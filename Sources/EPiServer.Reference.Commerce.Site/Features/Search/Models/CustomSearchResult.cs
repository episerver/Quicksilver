using EPiServer.Reference.Commerce.Site.Features.Product.ViewModels;
using Mediachase.Search;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Search.Models
{
    public class CustomSearchResult
    {
        public IEnumerable<ProductViewModel> ProductViewModels { get; set; }
        public ISearchResults SearchResult { get; set; }
        public IEnumerable<FacetGroupOption> FacetGroups { get; set; }
    }
}