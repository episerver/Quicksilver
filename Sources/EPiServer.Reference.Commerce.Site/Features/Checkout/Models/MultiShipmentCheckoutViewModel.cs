using EPiServer.Reference.Commerce.Site.Features.Cart.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Models
{
    public class MultiShipmentViewModel
    {
        public StartPage StartPage { get; set; }

        public IList<ShippingAddress> AvailableAddresses { get; set; }

        public CartItem[] CartItems { get; set; }

        public string ReferrerUrl { get; set; }
    }
}