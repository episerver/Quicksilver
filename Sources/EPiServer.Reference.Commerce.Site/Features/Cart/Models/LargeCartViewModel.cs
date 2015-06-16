using System;
using System.Collections.Generic;
using System.Linq;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.Models
{
    public class LargeCartViewModel
    {
        public IEnumerable<CartItem> CartItems { get; set; }
        public Money Total { get; set; }
        public Money TotalDiscount { get; set; }
    }
}