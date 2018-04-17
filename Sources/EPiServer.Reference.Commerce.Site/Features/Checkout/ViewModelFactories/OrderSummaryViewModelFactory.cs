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
        readonly IOrderGroupCalculator _orderGroupCalculator;
        readonly ICurrencyService _currencyService;

        public OrderSummaryViewModelFactory(IOrderGroupCalculator orderGroupCalculator, ICurrencyService currencyService)
        {
            _orderGroupCalculator = orderGroupCalculator;
            _currencyService = currencyService;
        }

        public virtual OrderSummaryViewModel CreateOrderSummaryViewModel(ICart cart)
        {
            if (cart == null)
            {
                return CreateEmptyOrderSummaryViewModel();
            }

            var totals = _orderGroupCalculator.GetOrderGroupTotals(cart);

            return new OrderSummaryViewModel
            {
                SubTotal = totals.SubTotal,
                CartTotal = totals.Total,
                ShippingTotal = totals.ShippingTotal,
                ShippingSubtotal = cart.GetShippingSubTotal(_orderGroupCalculator),
                OrderDiscountTotal = cart.GetOrderDiscountTotal(_orderGroupCalculator),
                ShippingDiscountTotal = cart.GetShippingDiscountTotal(),
                ShippingTaxTotal = totals.ShippingTotal + totals.TaxTotal,
                TaxTotal = totals.TaxTotal,
                OrderDiscounts = cart.GetFirstForm().Promotions.Where(x => x.DiscountType == DiscountType.Order).Select(x => new OrderDiscountViewModel
                {
                    Discount = new Money(x.SavedAmount, cart.Currency),
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