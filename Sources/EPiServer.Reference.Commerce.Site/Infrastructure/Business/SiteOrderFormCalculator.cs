using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Calculator;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.Business
{
    /// <summary>
    ///     This custom calculator extends the default one in order to 
    ///     exclude order level discounts from subtotal and instead include only in total.
    /// </summary>
    public class SiteOrderFormCalculator : DefaultOrderFormCalculator
    {
        private ILineItemCalculator _lineItemCalculator;

        public SiteOrderFormCalculator(IShippingCalculator shippingCalculator, ILineItemCalculator lineItemCalculator, ITaxCalculator taxCalculator) 
            : base(shippingCalculator, lineItemCalculator, taxCalculator)
        {
            _lineItemCalculator = lineItemCalculator;             
        }

        protected override Money CalculateSubtotal(IOrderForm orderForm, Currency currency)
        {
            var result = orderForm.GetAllLineItems().Where(x => !x.IsGift).Sum(lineItem => _lineItemCalculator.GetDiscountedPrice(lineItem, currency).Amount);
            return new Money(result, currency);
        }
    }
}