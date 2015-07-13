using System.Linq;
using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using Mediachase.Search;
using EPiServer.Reference.Commerce.Site.Features.Search.Services;

namespace EPiServer.Reference.Commerce.Site.Features.Search.Models
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

        public virtual SearchViewModel<T> Create<T>(T currentContent, FilterOptionFormModel formModel) where T : IContent
        {
            if (formModel.Q != null && (formModel.Q.StartsWith("*") || formModel.Q.StartsWith("?")))
            {
                return new SearchViewModel<T>
                {
                    CurrentContent = currentContent,
                    FormModel = formModel,
                    HasError = true,
                    ErrorMessage = _localizationService.GetString("/Search/BadFirstCharacter")
                };
            }

            var customSearchResult = _searchService.Search(currentContent, formModel);

            formModel.TotalCount = customSearchResult.SearchResult != null ? customSearchResult.SearchResult.TotalCount : 0;
            formModel.FacetGroups = customSearchResult.FacetGroups.ToList();

            formModel.Sorting = _searchService.GetSortOrder().Select(x => new SelectListItem
            {
                Text = _localizationService.GetString("/Category/Sort/" + x.Name),
                Value = x.Name.ToString(),
                Selected = string.Equals(x.Name.ToString(), formModel.Sort)
            });

            return new SearchViewModel<T>
            {
                CurrentContent = currentContent,
                ProductViewModels = customSearchResult.ProductViewModels,
                Facets = customSearchResult.SearchResult != null ? customSearchResult.SearchResult.FacetGroups : new ISearchFacetGroup[0],
                FormModel = formModel
            };
        }

    }
}