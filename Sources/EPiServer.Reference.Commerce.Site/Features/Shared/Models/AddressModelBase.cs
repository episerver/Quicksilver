using System.Collections.Generic;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using Mediachase.Commerce.Orders.Dto;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Models
{
    public class AddressModelBase
    {
        [LocalizedDisplay("/Shared/Address/Form/Label/FirstName")]
        [LocalizedRequired("/Shared/Address/Form/Empty/FirstName")]
        public virtual string FirstName { get; set; }

        [LocalizedDisplay("/Shared/Address/Form/Label/LastName")]
        [LocalizedRequired("/Shared/Address/Form/Empty/LastName")]
        public virtual string LastName { get; set; }

        public virtual string CountryName { get; set; }

        [LocalizedDisplay("/Shared/Address/Form/Label/CountryCode")]
        [LocalizedRequired("/Shared/Address/Form/Empty/CountryCode")]
        public string CountryCode { get; set; }

        public virtual IEnumerable<CountryDto.CountryRow> CountryOptions { get; set; }

        [LocalizedDisplay("/Shared/Address/Form/Label/City")]
        [LocalizedRequired("/Shared/Address/Form/Empty/City")]
        public virtual string City { get; set; }

        [LocalizedDisplay("/Shared/Address/Form/Label/PostalCode")]
        [LocalizedRequired("/Shared/Address/Form/Empty/PostalCode")]
        public virtual string PostalCode { get; set; }

        [LocalizedDisplay("/Shared/Address/Form/Label/Line1")]
        [LocalizedRequired("/Shared/Address/Form/Empty/Line1")]
        public virtual string Line1 { get; set; }

        public virtual IEnumerable<CountryDto.StateProvinceRow> RegionOptions { get; set; }

        [LocalizedDisplay("/Shared/Address/Form/Label/Region")]
        public virtual string Region { get; set; }
    }
}