using System.Collections.Generic;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Search.Models;
using EPiServer.Core;

namespace EPiServer.Reference.Commerce.Site.Features.Search.Services
{
    public interface ISearchService
    {
        CustomSearchResult Search(IContent currentContent, FilterOptionFormModel filterOptions);
        IEnumerable<ProductViewModel> QuickSearch(string query);
        IEnumerable<ProductViewModel> QuickSearch(FilterOptionFormModel filterOptions);
        IEnumerable<SortOrder> GetSortOrder();
    }
}