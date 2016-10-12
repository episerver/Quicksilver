using System;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels
{
    public class ShippingMethodViewModel
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public Money Price { get; set; }
    }
}