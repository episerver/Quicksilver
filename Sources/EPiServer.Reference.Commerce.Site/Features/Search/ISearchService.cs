using System.Collections.Generic;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Search.Models;
using EPiServer.Core;

namespace EPiServer.Reference.Commerce.Site.Features.Search
{
    public interface ISearchService
    {
        CustomSearchResult Search(IContent currentContent, FilterOptionFormModel filterOptions);
        IEnumerable<ProductViewModel> QuickSearch(string query, int pageSize = 5, string sort = "");
        IEnumerable<SortOrder> GetSortOrder();
    }
}