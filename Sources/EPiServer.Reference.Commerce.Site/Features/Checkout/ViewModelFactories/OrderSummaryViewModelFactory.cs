using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModelFactories
{
    [ServiceConfiguration(typeof(OrderSummaryViewModelFactory), Lifecycle = ServiceInstanceScope.Singleton)]
    public class OrderSummaryViewModelFactory 
    {
        readonly IOrderGroupTotalsCalculator _orderGroupTotalsCalculator;
        readonly IOrderGroupCalculator _orderGroupCalculator;
        readonly IShippingCalculator _shippingCalculator;
        readonly ICurrencyService _currencyService;

        public OrderSummaryViewModelFactory(
            IOrderGroupTotalsCalculator orderGroupTotalsCalculator, 
            IOrderGroupCalculator orderGroupCalculator, 
            IShippingCalculator shippingCalculator, 
            ICurrencyService currencyService)
        {
            _orderGroupTotalsCalculator = orderGroupTotalsCalculator;
            _orderGroupCalculator = orderGroupCalculator;
            _shippingCalculator = shippingCalculator;
            _currencyService = currencyService;
        }

        public virtual OrderSummaryViewModel CreateOrderSummaryViewModel(ICart cart)
        {
            if (cart == null)
            {
                return CreateEmptyOrderSummaryViewModel();
            }

            var totals = _orderGroupTotalsCalculator.GetTotals(cart);

            return new OrderSummaryViewModel
            {
                SubTotal = totals.SubTotal,
                CartTotal = totals.Total,
                ShippingTotal = totals.ShippingTotal,
                ShippingSubtotal = cart.GetShippingSubTotal(_shippingCalculator),
                OrderDiscountTotal = cart.GetOrderDiscountTotal(cart.Currency, _orderGroupCalculator),
                ShippingDiscountTotal = cart.GetShippingDiscountTotal(_shippingCalculator),
                ShippingTaxTotal = totals.ShippingTotal + totals.TaxTotal,
                TaxTotal = totals.TaxTotal,
                OrderDiscounts = cart.GetFirstForm().Promotions.Where(x => x.DiscountType == DiscountType.Order).Select(x => new OrderDiscountViewModel
                {
                    Discount = new Money(x.SavedAmount, new Currency(cart.Currency)),
                    DisplayName = x.Description
                })
            };
        }

        private OrderSummaryViewModel CreateEmptyOrderSummaryViewModel()
        {
            var zeroAmount = new Money(0, _currencyService.GetCurrentCurrency());
            return new OrderSummaryViewModel
            {
                CartTotal = zeroAmount,
                OrderDiscountTotal = zeroAmount,
                ShippingDiscountTotal = zeroAmount,
                ShippingSubtotal = zeroAmount,
                ShippingTaxTotal = zeroAmount,
                ShippingTotal = zeroAmount,
                SubTotal = zeroAmount,
                TaxTotal = zeroAmount,
                OrderDiscounts = Enumerable.Empty<OrderDiscountViewModel>(),
            };
        }
    }
}