using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Search.Pages;
using Mediachase.Commerce.Website.Search;
using Mediachase.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Search.Models
{
    public class SearchViewModelFactory
    {
        private readonly SearchService _searchService;
        private readonly LocalizationService _localizationService;

        public SearchViewModelFactory(LocalizationService localizationService, SearchService searchService)
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

            formModel.TotalCount = customSearchResult.SearchResult.TotalCount;
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
                Facets = customSearchResult.SearchResult.FacetGroups,
                FormModel = formModel
            };
        }

    }
}