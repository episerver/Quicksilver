using System.Collections.Generic;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using Mediachase.Commerce.Orders.Dto;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Models
{
    public class CountryRegion
    {
        public IEnumerable<CountryDto.StateProvinceRow> RegionOptions { get; set; }

        [LocalizedDisplay("/Shared/Address/Form/Label/CountryRegion")]
        public string Region { get; set; }
    }
}