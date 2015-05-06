using Mediachase.Commerce;
using System;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Models
{
    public class ShippingMethodViewModel
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public Money Price { get; set; }
    }
}