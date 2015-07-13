using Mediachase.Commerce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.Models
{
    public abstract class CartViewModel
    {
        public decimal ItemCount { get; set; }
        public IEnumerable<CartItem> CartItems { get; set; }
        public Money Total { get; set; }
    }
}