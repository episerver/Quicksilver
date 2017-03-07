using EPiServer.Reference.Commerce.Site.Features.Search.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Search.ViewModels
{
    [ModelBinder(typeof(FilterOptionViewModelBinder))]
    public class FilterOptionViewModel
    {
        public List<FacetGroupOption> FacetGroups { get; set; }
        public string SelectedFacet { get; set; }
        public IEnumerable<SelectListItem> Sorting { get; set; }
        public string Sort { get; set; }
        public int Page { get; set; }
        public string Q { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
    }
}