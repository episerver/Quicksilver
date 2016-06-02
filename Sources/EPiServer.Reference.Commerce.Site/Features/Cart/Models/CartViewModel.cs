using Mediachase.Commerce;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.Models
{
    public abstract class CartViewModel
    {
        public decimal ItemCount { get; set; }

        public CartItem[] CartItems { get; set; }

        public Money Total { get; set; }
    }
}