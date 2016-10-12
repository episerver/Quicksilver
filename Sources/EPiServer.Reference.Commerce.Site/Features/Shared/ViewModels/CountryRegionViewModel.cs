using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.ViewModels
{
    public class CountryRegionViewModel
    {
        public IEnumerable<string> RegionOptions { get; set; }

        [LocalizedDisplay("/Shared/Address/Form/Label/CountryRegion")]
        public string Region { get; set; }
    }
}