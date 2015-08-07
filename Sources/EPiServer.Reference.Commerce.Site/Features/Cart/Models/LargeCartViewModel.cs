using System;
using System.Collections.Generic;
using System.Linq;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.Models
{
    public class LargeCartViewModel : CartViewModel
    {
        public Money TotalDiscount { get; set; }
    }
}