using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Calculator;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.Business
{
    public class SiteOrderGroupCalculator : DefaultOrderGroupCalculator
    {
        private readonly IShippingCalculator _shippingCalculator;
        private readonly ITaxCalculator _taxCalculator;

        public SiteOrderGroupCalculator(IOrderFormCalculator orderFormCalculator, IShippingCalculator shippingCalculator, ITaxCalculator taxCalculator) 
            : base(orderFormCalculator, shippingCalculator, taxCalculator)
        {
            _shippingCalculator = shippingCalculator;
            _taxCalculator = taxCalculator;
        }
        
        protected override Money CalculateTotal(IOrderGroup orderGroup)
        {
            var result = GetSubTotal(orderGroup) + GetHandlingTotal(orderGroup) - GetOrderDiscountTotal(orderGroup, orderGroup.Currency);
            result +=
                _shippingCalculator.GetShippingCost(orderGroup, orderGroup.Market, orderGroup.Currency) +
                _taxCalculator.GetTaxTotal(orderGroup, orderGroup.Market, orderGroup.Currency);

            return result;
        }
    }
}