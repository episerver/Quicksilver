using System.Collections.Generic;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using Mediachase.Search;

namespace EPiServer.Reference.Commerce.Site.Features.Search.Models
{
    public class CustomSearchResult
    {
        public IEnumerable<ProductViewModel> ProductViewModels { get; set; }
        public ISearchResults SearchResult { get; set; }
        public IEnumerable<FacetGroupOption> FacetGroups { get; set; }
    }
}