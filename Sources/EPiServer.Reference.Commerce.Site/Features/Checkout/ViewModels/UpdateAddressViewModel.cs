using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels
{
    public class UpdateAddressViewModel
    {
        public AddressModel BillingAddress { get; set; }
        public CheckoutPage CurrentPage { get; set; }
        public int ShippingAddressIndex { get; set; }
        public bool UseBillingAddressForShipment { get; set; }
        public IList<ShipmentViewModel> Shipments { get; set; }
    }
}