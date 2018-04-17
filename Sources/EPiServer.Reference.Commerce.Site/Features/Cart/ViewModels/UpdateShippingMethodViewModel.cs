using System;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels
{
    public class UpdateShippingMethodViewModel
    {
        public int ShipmentId { get; set; }

        public Guid ShippingMethodId { get; set; }
    }
}