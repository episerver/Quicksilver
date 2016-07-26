using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using Mediachase.Commerce.Orders.Dto;
using System;
using System.Collections.Generic;
using EPiServer.Web;
using System.ComponentModel.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Models
{
    public class Address
    {
        public Address()
        {
            CountryRegion = new CountryRegion();
        }

        public bool SaveAddress { get; set; }

        public Guid? AddressId { get; set; }

        public DateTime? Modified { get; set; }

        [LocalizedRequired("/Shared/Address/Form/Empty/Name")]
        [LocalizedDisplay("/Shared/Address/Form/Label/Name")]
        public string Name { get; set; }

        [LocalizedDisplay("/Shared/Address/Form/Label/FirstName")]
        [LocalizedRequired("/Shared/Address/Form/Empty/FirstName")]
        public string FirstName { get; set; }

        [LocalizedDisplay("/Shared/Address/Form/Label/LastName")]
        [LocalizedRequired("/Shared/Address/Form/Empty/LastName")]
        public string LastName { get; set; }

        public string CountryName { get; set; }

        [LocalizedDisplay("/Shared/Address/Form/Label/CountryCode")]
        [LocalizedRequired("/Shared/Address/Form/Empty/CountryCode")]
        public string CountryCode { get; set; }

        public IEnumerable<CountryDto.CountryRow> CountryOptions { get; set; }

        [LocalizedDisplay("/Shared/Address/Form/Label/City")]
        [LocalizedRequired("/Shared/Address/Form/Empty/City")]
        public string City { get; set; }

        [LocalizedDisplay("/Shared/Address/Form/Label/PostalCode")]
        [LocalizedRequired("/Shared/Address/Form/Empty/PostalCode")]
        public string PostalCode { get; set; }

        [LocalizedDisplay("/Shared/Address/Form/Label/Line1")]
        [LocalizedRequired("/Shared/Address/Form/Empty/Line1")]
        public string Line1 { get; set; }

        [LocalizedDisplay("/Shared/Address/Form/Label/Line2")]
        public string Line2 { get; set; }

        [UIHint("AddressRegion")]
        public CountryRegion CountryRegion { get; set; }

        [LocalizedDisplay("/Shared/Address/Form/Label/Email")]
        [LocalizedEmail("/Shared/Address/Form/Error/InvalidEmail")]
        public string Email { get; set; }

        [LocalizedDisplay("/Shared/Address/Form/Label/ShippingAddress")]
        public bool ShippingDefault { get; set; }

        [LocalizedDisplay("/Shared/Address/Form/Label/BillingAddress")]
        public bool BillingDefault { get; set; }

        [LocalizedDisplay("/Shared/Address/Form/Label/DaytimePhoneNumber")]
        public string DaytimePhoneNumber { get; set; }

        [LocalizedDisplay("/Shared/Address/Form/Label/Organization")]
        public string Organization { get; set; }

        public string ErrorMessage { get; set; }
    }
}