using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using System;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels
{
    public class ShipmentViewModel
	{
        public int ShipmentId { get; set; }

        public IList<CartItemViewModel> CartItems { get; set; }

        public AddressModel Address { get; set; }

        public Guid ShippingMethodId { get; set; }

        public IEnumerable<ShippingMethodViewModel> ShippingMethods { get; set; }
    }
}