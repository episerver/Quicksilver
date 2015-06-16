using System;
using System.ComponentModel.DataAnnotations;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Models
{
    public class AddressFormModel : AddressModelBase
    {
        [LocalizedDisplay("/Checkout/Address/Labels/Email")]
        [LocalizedEmail("/Registration/Form/Error/InvalidEmail")]
        [LocalizedRequired("/Checkout/Address/Empty/Email")]
        public string Email { get; set; }

        [LocalizedDisplay("/Checkout/Address/Labels/AddressId")]
        public Guid AddressId { get; set; }

        public bool SaveAddress { get; set; }
    }
}