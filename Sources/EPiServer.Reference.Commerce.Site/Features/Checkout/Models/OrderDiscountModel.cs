using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Models
{
    public class OrderDiscountModel
    {
        public Money Discount { get; set; }
        public String Displayname { get; set; }
    }
}