using System.Web.Helpers;

namespace EPiServer.Reference.Commerce.Site.Features.Search.Models
{
    public class SortOrder
    {
        public ProductSortOrder Name { get; set; }
        public string Key { get; set; }
        public SortDirection SortDirection { get; set; }
    }
}