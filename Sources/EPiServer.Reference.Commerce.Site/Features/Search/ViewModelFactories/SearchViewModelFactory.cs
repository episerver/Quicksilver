using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Personalization.Commerce.Tracking;
using EPiServer.Reference.Commerce.Site.Features.Search.Services;
using EPiServer.Reference.Commerce.Site.Features.Search.ViewModels;
using Mediachase.Search;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ISearchService = EPiServer.Reference.Commerce.Site.Features.Search.Services.ISearchService;

namespace EPiServer.Reference.Commerce.Site.Features.Search.ViewModelFactories
{
    public class SearchViewModelFactory
    {
        private readonly ISearchService _searchService;
        private readonly LocalizationService _localizationService;

        public SearchViewModelFactory(LocalizationService localizationService, ISearchService searchService)
        {
            _searchService = searchService;
            _localizationService = localizationService;
        }

        public virtual SearchViewModel<T> Create<T>(T currentContent, FilterOptionViewModel viewModel) where T : IContent
        {
            if (viewModel.Q != null && (viewModel.Q.StartsWith("*") || viewModel.Q.StartsWith("?")))
            {
                return new SearchViewModel<T>
                {
                    CurrentContent = currentContent,
                    FilterOption = viewModel,
                    HasError = true,
                    ErrorMessage = _localizationService.GetString("/Search/BadFirstCharacter"),
                    Recommendations = new List<Recommendation>()
                };
            }

            var customSearchResult = _searchService.Search(currentContent, viewModel);

            var totalResultCount = customSearchResult.SearchResult?.TotalCount ?? 0;

            viewModel.TotalCount = totalResultCount;
            viewModel.FacetGroups = customSearchResult.FacetGroups.ToList();

            viewModel.Sorting = _searchService.GetSortOrder().Select(x => new SelectListItem
            {
                Text = _localizationService.GetString("/Category/Sort/" + x.Name),
                Value = x.Name.ToString(),
                Selected = string.Equals(x.Name.ToString(), viewModel.Sort)
            });

            return new SearchViewModel<T>
            {
                CurrentContent = currentContent,
                ProductViewModels = customSearchResult.ProductViewModels,
                TotalResultCount = totalResultCount,
                Facets = customSearchResult.SearchResult != null ? customSearchResult.SearchResult.FacetGroups : new ISearchFacetGroup[0],
                FilterOption = viewModel,
                Recommendations = new List<Recommendation>()
            };
        }

    }
}