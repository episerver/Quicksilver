using EPiServer.Reference.Commerce.Site.Features.Checkout.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using Mediachase.Commerce;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.Models
{
    public class LargeCartViewModel : CartViewModel
    {
        public Money TotalDiscount { get; set; }
    }
}