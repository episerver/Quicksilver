using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels
{
    public class UpdateShippingMethodViewModel
    {
        public IList<ShipmentViewModel> Shipments { get; set; }
    }
}