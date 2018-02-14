using System.Collections.Generic;
using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels
{
    public class ShipmentConfirmationViewModel
    {
        public IEnumerable<ILineItem> LineItems { get; set; }
        public Money ShipmentCost { get; set; }
        public AddressModel Address { get; set; }
        public Money DiscountPrice { get; set; }
        public Money ShippingItemsTotal { get; set; }
        public string ShippingMethodName { get; set; }
    }
}