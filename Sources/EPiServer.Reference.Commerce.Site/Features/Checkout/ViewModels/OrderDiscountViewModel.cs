using System;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels
{
    public class OrderDiscountViewModel
    {
        public Money Discount { get; set; }
        public String DisplayName { get; set; }
    }
}