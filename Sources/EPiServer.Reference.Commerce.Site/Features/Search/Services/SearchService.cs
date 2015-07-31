using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Search.Models;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Reference.Commerce.Site.Infrastructure.Indexing;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Lucene.Net.QueryParsers;
using Lucene.Net.Util;
using Mediachase.Commerce;
using Mediachase.Search;
using Mediachase.Search.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Helpers;

namespace EPiServer.Reference.Commerce.Site.Features.Search.Services
{
    [ServiceConfiguration(typeof(ISearchService), Lifecycle = ServiceInstanceScope.PerRequest)]
    public class SearchService : ISearchService
    {
        private readonly SearchFacade _search;
        private readonly ICurrentMarket _currentMarket;
        private readonly ICurrencyService _currencyService;
        private readonly UrlResolver _urlResolver;
        private readonly CultureInfo _preferredCulture;
        private readonly IContentLoader _contentLoader;
        private readonly LocalizationService _localizationService;

        public SearchService(ICurrentMarket currentMarket, 
            ICurrencyService currencyService, 
            UrlResolver urlResolver, 
            SearchFacade search,
            Func<CultureInfo> preferredCulture,
            IContentLoader contentLoader,
            LocalizationService localizationService)
        {
            _search = search;
            _currentMarket = currentMarket;
            _currencyService = currencyService;
            _urlResolver = urlResolver;
            _preferredCulture = preferredCulture();
            _contentLoader = contentLoader;
            _localizationService = localizationService;
        }

        public CustomSearchResult Search(IContent currentContent, FilterOptionFormModel filterOptions)
        {
            if (filterOptions == null)
            {
                return CreateEmptyResult();
            }

            var criteria = CreateCriteria(currentContent, filterOptions);
            AddFacets(filterOptions.FacetGroups, criteria, currentContent);
            return Search(criteria, currentContent);
        }

        public IEnumerable<ProductViewModel> QuickSearch(string query)
        {
            var filterOptions = new FilterOptionFormModel
            {
                Q = query,
                PageSize = 5,
                Sort = string.Empty
            };
            return QuickSearch(filterOptions);
        }

        public IEnumerable<ProductViewModel> QuickSearch(FilterOptionFormModel filterOptions)
        {
            if (String.IsNullOrEmpty(filterOptions.Q))
            {
                return Enumerable.Empty<ProductViewModel>();
            }

            var criteria = CreateCriteriaForQuickSearch(filterOptions);

            try
            {
                var searchResult = _search.Search(criteria);
                return CreateProductViewModels(searchResult);
            }
            catch (ParseException)
            {
                return new ProductViewModel[0];
            }
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

        private CatalogEntrySearchCriteria CreateCriteriaForQuickSearch(FilterOptionFormModel filterOptions)
        {
            var sortOrder = GetSortOrder().FirstOrDefault(x => x.Name.ToString() == filterOptions.Sort) ?? GetSortOrder().First();
            var market = _currentMarket.GetCurrentMarket();

            var criteria = new CatalogEntrySearchCriteria
            {
                ClassTypes = new StringCollection { "product" },
                Locale = _preferredCulture.Name,
                MarketId = market.MarketId,
                StartingRecord = 0,
                RecordsToRetrieve = filterOptions.PageSize,
                Sort = new SearchSort(new SearchSortField(sortOrder.Key, sortOrder.SortDirection == SortDirection.Descending)),
                SearchPhrase = GetEscapedSearchPhrase(filterOptions.Q)
            };

            return criteria;
        }

        private CatalogEntrySearchCriteria CreateCriteria(IContent currentContent, FilterOptionFormModel filterOptions)
        {
            var pageSize = filterOptions.PageSize > 0 ? filterOptions.PageSize : 20;
            var sortOrder = GetSortOrder().FirstOrDefault(x => x.Name.ToString() == filterOptions.Sort) ?? GetSortOrder().First();
            var market = _currentMarket.GetCurrentMarket();

            var criteria = new CatalogEntrySearchCriteria
            {
                ClassTypes = new StringCollection { "product" },
                Locale = _preferredCulture.Name,
                MarketId = market.MarketId,
                StartingRecord = pageSize * (filterOptions.Page - 1),
                RecordsToRetrieve = pageSize,
                Sort = new SearchSort(new SearchSortField(sortOrder.Key, sortOrder.SortDirection == SortDirection.Descending))
            };
            
            var nodeContent = currentContent as NodeContent;
            if (nodeContent != null)
            {
                criteria.Outlines = _search.GetOutlinesForNode(nodeContent.Code);
            }
            if (!string.IsNullOrEmpty(filterOptions.Q))
            {
                criteria.SearchPhrase = GetEscapedSearchPhrase(filterOptions.Q);
            }

            return criteria;
        }

        private void AddFacets(List<FacetGroupOption> facetGroups, CatalogEntrySearchCriteria criteria, IContent currentContent)
        {
            var nodeContent = currentContent as NodeContent;
            if (facetGroups == null && nodeContent == null)
            {
                return;
            }

            foreach (var facetGroupOption in facetGroups.Where(x => x.Facets.Any(y => y.Selected)))
            {
                var searchFilter = _search.SearchFilters.FirstOrDefault(x => x.field.Equals(facetGroupOption.GroupFieldName, StringComparison.OrdinalIgnoreCase));
                if (searchFilter == null)
                {
                    if (nodeContent == null)
                    {
                        continue;
                    }
                    searchFilter = GetSearchFilterForNode(nodeContent);
                }
                
                var facetValues = searchFilter.Values.SimpleValue
                    .Where(x => facetGroupOption.Facets.FirstOrDefault(y => y.Selected && y.Key.ToLower() == x.key.ToLower()) != null);

                criteria.Add(searchFilter.field.ToLower(), facetValues);
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

        private CustomSearchResult Search(CatalogEntrySearchCriteria criteria, IContent currentContent)
        {
            var nodeContent = currentContent as NodeContent;
            if (nodeContent != null)
            {
                criteria.Add(GetSearchFilterForNode(nodeContent));
            }
            _search.SearchFilters.ToList().ForEach(criteria.Add);
            
            ISearchResults searchResult;

            try
            {
                searchResult = _search.Search(criteria);
            }
            catch (ParseException)
                {
                return new CustomSearchResult
                {
                    FacetGroups = new List<FacetGroupOption>(),
                    ProductViewModels = new List<ProductViewModel>()
                };
            }

            var facetGroups = new List<FacetGroupOption>();
            foreach (var searchFacetGroup in searchResult.FacetGroups)
            {
                // Only add facet group if more than one value is available
                if (searchFacetGroup.Facets.Count == 0)
                {
                    continue;
                }
                facetGroups.Add(new FacetGroupOption
                {
                    GroupName = searchFacetGroup.Name,
                    GroupFieldName = searchFacetGroup.FieldName,
                    Facets = searchFacetGroup.Facets.OfType<Facet>().Select(y => new FacetOption
                    {
                        Name = y.Name,
                        Selected = y.IsSelected,
                        Count = y.Count,
                        Key = y.Key 
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

        private IEnumerable<ProductViewModel> CreateProductViewModels(ISearchResults searchResult)
        {
            var market = _currentMarket.GetCurrentMarket();
            var currency = _currencyService.GetCurrentCurrency();

            return searchResult.Documents.Select(document => new ProductViewModel
            {
                Brand = GetString(document, "brand"),
                Code = GetString(document, "code"),
                DisplayName = GetString(document, "displayname"),
                PlacedPrice = new Money(GetDecimal(document, IndexingHelper.GetOriginalPriceField(market.MarketId, currency)), currency),
                ExtendedPrice = new Money(GetDecimal(document, IndexingHelper.GetPriceField(market.MarketId, currency)), currency),
                ImageUrl = GetString(document, "image_url"),
                Url = _urlResolver.GetUrl(ContentReference.Parse(GetString(document, "content_link")))
            });
        }
        
        private static string GetString(ISearchDocument document, string name)
        {
            return document[name] != null ? document[name].Value.ToString() : "";
        }

        private decimal GetDecimal(ISearchDocument document, string name)
        {
            if (document[name] == null)
            {
                return 0m;
            }

            return _search.GetSearchProvider() == SearchFacade.SearchProviderType.Lucene
                ? Convert.ToDecimal(NumericUtils.PrefixCodedToLong(document[name].Value.ToString()) / 10000m)
                : decimal.Parse(document[name].Value.ToString(), CultureInfo.InvariantCulture.NumberFormat);
        }

        private static string GetEscapedSearchPhrase(string query)
        {
            var searchPhrase = RemoveInvalidCharacters(query);
            if (String.IsNullOrEmpty(searchPhrase))
            {
                return string.Empty;
            }

            return String.Concat(searchPhrase, "*");
        }

        private static string RemoveInvalidCharacters(string s)
        {
            var stringBuilder = new StringBuilder();
            foreach (var ch in s)
            {
                switch (ch)
                {
                    case '\\':
                    case '+':
                    case '!':
                    case '(':
                    case ')':
                    case ':':
                    case '^':
                    case '[':
                    case ']':
                    case '"':
                    case '{':
                    case '}':
                    case '~':
                    case '*':
                    case '?':
                    case '|':
                    case '&':
                        continue;
                }

                stringBuilder.Append(ch);
            }

            return stringBuilder.ToString().Trim();
        }

        public SearchFilter GetSearchFilterForNode(NodeContent nodeContent)
        {
            var configFilter = new SearchFilter
            {
                field = BaseCatalogIndexBuilder.FieldConstants.Node,
                Descriptions = new Descriptions
                {
                    defaultLocale = _preferredCulture.Name
                },
                Values = new SearchFilterValues()
            };

            var desc = new Description
            {
                locale = "en",
                Value = _localizationService.GetString("/Facet/Category")
            };
            configFilter.Descriptions.Description = new[] { desc };  

            var nodes = _contentLoader.GetChildren<NodeContent>(nodeContent.ContentLink).ToList();
            var nodeValues = new SimpleValue[nodes.Count];
            var index = 0;
            foreach (var node in nodes)
            {
                var val = new SimpleValue
                {
                    key = node.Code,
                    value = node.Code,
                    Descriptions = new Descriptions
                    {
                        defaultLocale = _preferredCulture.Name
                    }
                };
                var desc2 = new Description
                {
                    locale = _preferredCulture.Name,
                    Value = node.DisplayName
                };
                val.Descriptions.Description = new[] { desc2 };

                nodeValues[index] = val;
                index++;
            }
            configFilter.Values.SimpleValue = nodeValues;
            return configFilter;
        }
    }
}