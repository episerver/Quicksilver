using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.Reference.Commerce.LuceneSearchProvider;
using EPiServer.Reference.Commerce.Site.Features.Market;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Search.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Search.Models;
using EPiServer.Reference.Commerce.Site.Infrastructure.Indexing;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Website.Search;
using Mediachase.Search;
using Mediachase.Search.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Helpers;

namespace EPiServer.Reference.Commerce.Site.Features.Search
{
    [ServiceConfiguration(typeof(ISearchService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class SearchService : ISearchService
    {
        private readonly SearchManager _searchManager;
        private readonly ICurrentMarket _currentMarket;
        private readonly CurrencyService _currencyService;
        private readonly UrlResolver _urlResolver;

        public SearchService(ICurrentMarket currentMarket, CurrencyService currencyService, UrlResolver urlResolver)
        {
            _searchManager = new SearchManager(AppContext.Current.ApplicationName);
            _currentMarket = currentMarket;
            _currencyService = currencyService;
            _urlResolver = urlResolver;
        }

        public CustomSearchResult Search(IContent currentContent, FilterOptionFormModel filterOptions)
        {
            if (filterOptions == null)
            {
                return CreateEmptyResult();
            }
            var pageSize = filterOptions.PageSize > 0 ? filterOptions.PageSize : 20;
            var criteria = CreateCriteria(pageSize, filterOptions.Sort, filterOptions.Page, filterOptions.FacetGroups);
            var nodeContent = currentContent as NodeContent;
            if (nodeContent != null)
            {
                criteria.Outlines = SearchFilterHelper.GetOutlinesForNode(nodeContent.Code);
            }
            if (!string.IsNullOrEmpty(filterOptions.Q))
            {
                criteria.SearchPhrase = filterOptions.Q + "*";
            }
            
            return Search(criteria);
        }
        
        private MultiFacetSearchCriteria CreateCriteria(int pageSize, string sort, int page, List<FacetGroupOption> facetGroups)
        {
            var sortOrder = GetSortOrder().FirstOrDefault(x => x.Name.ToString() == sort) ?? GetSortOrder().First();
            var criteria = new MultiFacetSearchCriteria
            {
                ClassTypes = new StringCollection { "product" },
                Locale = ContentLanguage.PreferredCulture.Name,
                StartingRecord = pageSize * (page - 1),
                RecordsToRetrieve = pageSize,
                Sort = new SearchSort(new SearchSortField(sortOrder.Key, sortOrder.SortDirection == SortDirection.Descending))
            };
            AddFacets(facetGroups, criteria);

            return criteria;
        }

        private static void AddFacets(List<FacetGroupOption> facetGroups, MultiFacetSearchCriteria criteria)
        {
            if (facetGroups != null)
            {
                foreach (var facetGroupOption in facetGroups.Where(x => x.Facets.Any(y => y.Selected)))
                {
                    var searchFilter = SearchFilterHelper.Current.SearchConfig.SearchFilters.FirstOrDefault(x => x.field.Equals(facetGroupOption.GroupFieldName, StringComparison.OrdinalIgnoreCase));
                    if (searchFilter == null)
                    {
                        continue;
                    }

                    var facetValues = searchFilter.Values.SimpleValue.Where(x => facetGroupOption.Facets.FirstOrDefault(y => y.Selected && y.Name.ToLower() == x.value.ToLower()) != null);

                    criteria.AddMultiSelectActiveField(searchFilter.field.ToLower(), facetValues);
                }
            }
        }

        private static CustomSearchResult CreateEmptyResult()
        {
            return new CustomSearchResult
            {
                ProductViewModels = Enumerable.Empty<ProductViewModel>(),
                FacetGroups = Enumerable.Empty<FacetGroupOption>(),
                SearchResult = new SearchResults(null, null)
                {
                    FacetGroups = Enumerable.Empty<ISearchFacetGroup>().ToArray()
                }
            };
        }

        private CustomSearchResult Search(MultiFacetSearchCriteria criteria)
        {
            SearchFilterHelper.Current.SearchConfig.SearchFilters.ToList().ForEach(criteria.Add);

            var searchResult = _searchManager.Search(criteria);

            var facetGroups = new List<FacetGroupOption>();
            foreach (var searchFacetGroup in searchResult.FacetGroups)
            {
                // Only add facet group if more than one value is available
                if (searchFacetGroup.Facets.Count <= 1)
                {
                    continue;
                }
                facetGroups.Add(new FacetGroupOption
                {
                    GroupName = searchFacetGroup.Name,
                    GroupFieldName = searchFacetGroup.FieldName,
                    Facets = searchFacetGroup.Facets.OfType<MultiFacet>().Select(y => new FacetOption
                    {
                        Name = y.Name,
                        Selected = y.IsSelected,
                        Count = y.Count
                    }).ToList()
                });
            }

            return new CustomSearchResult
            {
                ProductViewModels = CreateProductViewModels(searchResult),
                SearchResult = searchResult,
                FacetGroups = facetGroups
            };
        }

        public IEnumerable<ProductViewModel> QuickSearch(string query, int pageSize = 5, string sort = "")
        {
            if (String.IsNullOrEmpty(query))
            {
                return Enumerable.Empty<ProductViewModel>();
            }

            var sortOrder = GetSortOrder().FirstOrDefault(x => x.Name.ToString() == sort) ?? GetSortOrder().First();
            var criteria = new MultiFacetSearchCriteria
            {
                ClassTypes = new StringCollection { "product" },
                Locale = ContentLanguage.PreferredCulture.Name,
                StartingRecord = 0,
                RecordsToRetrieve = pageSize,
                Sort = new SearchSort(new SearchSortField(sortOrder.Key, sortOrder.SortDirection == SortDirection.Descending)),
                SearchPhrase = query
            };

            var searchResult = _searchManager.Search(criteria);
            return CreateProductViewModels(searchResult);
        }

        public IEnumerable<SortOrder> GetSortOrder()
        {
            var market = _currentMarket.GetCurrentMarket();
            var currency = _currencyService.GetCurrentCurrency();

            return new List<SortOrder>
            {
                new SortOrder {Name = ProductSortOrder.PriceAsc, Key = IndexingHelper.GetPriceField(market.MarketId, currency), SortDirection = SortDirection.Ascending},
                new SortOrder {Name = ProductSortOrder.Popularity, Key = "", SortDirection = SortDirection.Ascending},
                new SortOrder {Name = ProductSortOrder.NewestFirst, Key = "created", SortDirection = SortDirection.Descending}
            };
        }

        private IEnumerable<ProductViewModel> CreateProductViewModels(ISearchResults searchResult)
        {
            var market = _currentMarket.GetCurrentMarket();
            var currency = _currencyService.GetCurrentCurrency();

            return searchResult.Documents.Select(document => new ProductViewModel
            {
                DisplayName = document.GetString("displayname"),
                OriginalPrice = new Money(document.GetDecimal(IndexingHelper.GetOriginalPriceField(market.MarketId, currency)), currency),
                    Price = new Money(document.GetDecimal(IndexingHelper.GetPriceField(market.MarketId, currency)), currency),
                    Image = document.GetString("image_url"),
                    Url = _urlResolver.GetUrl(ContentReference.Parse(document.GetString("content_link")))
            });
        }
    }
}