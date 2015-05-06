using System.Collections.Generic;
using EPiServer.Core;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.Models
{
    public class MiniCartDetailsViewModel
    {
        public int ItemCount { get; set; }
        public ContentReference CheckoutPage { get; set; }
        public IEnumerable<CartItem> CartItems { get; set; }
        public Money Total { get; set; }
    }
}