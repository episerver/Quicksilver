using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using Mediachase.Search;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Search.Models
{
    public class SearchViewModel<T> where T : IContent
    {
        public IEnumerable<ProductViewModel> ProductViewModels { get; set; }
        public T CurrentContent { get; set; }
        public FilterOptionFormModel FormModel { get; set; }
        public ISearchFacetGroup[] Facets { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
    }
}