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
        IEnumerable<ProductViewModel> QuickSearch(string query);
        IEnumerable<ProductViewModel> QuickSearch(FilterOptionFormModel filterOptions);
        IEnumerable<SortOrder> GetSortOrder();
    }
}