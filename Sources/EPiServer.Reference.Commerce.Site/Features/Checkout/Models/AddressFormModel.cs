using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Models
{
    public class AddressFormModel
    {
        [LocalizedDisplay("/Checkout/Address/Labels/FirstName")]
        [LocalizedRequired("/Checkout/Address/Empty/FirstName")]
        public string FirstName { get; set; }

        [LocalizedDisplay("/Checkout/Address/Labels/LastName")]
        [LocalizedRequired("/Checkout/Address/Empty/LastName")]
        public string LastName { get; set; }

        [LocalizedDisplay("/Checkout/Address/Labels/Email")]
        [LocalizedEmail("/Registration/Form/Error/InvalidEmail")]
        [LocalizedRequired("/Checkout/Address/Empty/Email")]
        public string Email { get; set; }

        [LocalizedDisplay("/Checkout/Address/Labels/Address")]
        [LocalizedRequired("/Checkout/Address/Empty/Address")]
        public string Address { get; set; }

        [LocalizedDisplay("/Checkout/Address/Labels/Country")]
        [LocalizedRequired("/Checkout/Address/Empty/Country")]
        public string Country { get; set; }

        [LocalizedDisplay("/Checkout/Address/Labels/City")]
        [LocalizedRequired("/Checkout/Address/Empty/City")]
        public string City { get; set; }

        [LocalizedDisplay("/Checkout/Address/Labels/ZipCode")]
        [LocalizedRequired("/Checkout/Address/Empty/ZipCode")]
        public string ZipCode { get; set; }

        [LocalizedDisplay("/Checkout/Address/Labels/AddressId")]
        public Guid AddressId { get; set; }

        public bool SaveAddress { get; set; }
    }
}