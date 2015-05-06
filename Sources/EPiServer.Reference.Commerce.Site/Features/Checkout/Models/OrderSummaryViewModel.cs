using System.Collections;
using System.Collections.Generic;
using Mediachase.Commerce;
namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Models
{
    public class OrderSummaryViewModel
    {
        public Money SubTotal { get; set; }
        public Money HandlingTotal { get; set; }
        public IEnumerable<OrderDiscountModel> OrderDiscounts { get; set; }
        public Money OrderDiscountTotal { get; set; }
        public Money ShippingDiscountTotal { get; set; }
        public Money ShippingTotal { get; set; }
        public Money ShippingSubtotal { get; set; }
        public IEnumerable<OrderDiscountModel> ShippingDiscounts { get; set; }
        public Money TaxTotal { get; set; }
        public Money ShippingTaxTotal { get; set; }
        public Money CartTotal { get; set; }
    }
}