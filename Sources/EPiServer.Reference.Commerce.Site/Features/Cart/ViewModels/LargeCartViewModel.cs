using System.Collections.Generic;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels
{
    public class LargeCartViewModel
    {
        public IEnumerable<ShipmentViewModel> Shipments { get; set; }

        public Money TotalDiscount { get; set; }

        public Money Total { get; set; }
    }
}