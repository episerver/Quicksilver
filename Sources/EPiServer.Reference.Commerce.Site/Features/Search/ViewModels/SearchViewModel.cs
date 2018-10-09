using EPiServer.Core;
using EPiServer.Personalization.Commerce.Tracking;
using EPiServer.Reference.Commerce.Site.Features.Product.ViewModels;
using Mediachase.Search;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Search.ViewModels
{
    public class SearchViewModel<T> where T : IContent
    {
        public IEnumerable<ProductTileViewModel> ProductViewModels { get; set; }
        public int TotalResultCount { get; set; }
        public IEnumerable<Recommendation> Recommendations { get; set; }
        public T CurrentContent { get; set; }
        public FilterOptionViewModel FilterOption { get; set; }
        public ISearchFacetGroup[] Facets { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
    }
}