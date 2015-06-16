using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using EPiServer.Core;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.Models
{
    public class MiniCartViewModel
    {
        public decimal ItemCount { get; set; }
        public ContentReference CheckoutPage { get; set; }
        public IEnumerable<CartItem> CartItems { get; set; }
        public Money Total { get; set; }
    }
}