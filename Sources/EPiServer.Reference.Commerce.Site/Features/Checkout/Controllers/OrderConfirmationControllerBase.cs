using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Web.Mvc;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Controllers
{
    public abstract class OrderConfirmationControllerBase<T> : PageController<T> where T : PageData
    {
        protected readonly ConfirmationService ConfirmationService;
        protected readonly CustomerContextFacade CustomerContext;
        private readonly AddressBookService _addressBookService;
        private readonly IOrderGroupCalculator _orderGroupCalculator;
        private readonly IMarketService _marketService;

        protected OrderConfirmationControllerBase(
            ConfirmationService confirmationService,
            AddressBookService addressBookService,
            CustomerContextFacade customerContextFacade,
            IOrderGroupCalculator orderGroupCalculator,
            IMarketService marketService)
        {
            ConfirmationService = confirmationService;
            _addressBookService = addressBookService;
            CustomerContext = customerContextFacade;
            _orderGroupCalculator = orderGroupCalculator;
            _marketService = marketService;
        }

        protected OrderConfirmationViewModel<T> CreateViewModel(T currentPage, IPurchaseOrder order)
        {
            if (order == null)
            {
                return new OrderConfirmationViewModel<T> { CurrentPage = currentPage };
            }

            var totals = order.GetOrderGroupTotals(_orderGroupCalculator);

            return new OrderConfirmationViewModel<T>
            {
                Currency = order.Currency,
                CurrentPage = currentPage,
                HasOrder = true,
                OrderId = order.OrderNumber,
                Created = order.Created,
                BillingAddress = _addressBookService.ConvertToModel(order.GetFirstForm().Payments.First().BillingAddress),
                ContactId = CustomerContext.CurrentContactId,
                Payments = order.GetFirstForm().Payments.Where(c => c.TransactionType == TransactionType.Authorization.ToString() || c.TransactionType == TransactionType.Sale.ToString()),
                OrderGroupId = order.OrderLink.OrderGroupId,
                OrderLevelDiscountTotal = order.GetOrderDiscountTotal(),
                ShippingSubTotal = order.GetShippingSubTotal(),
                ShippingDiscountTotal = order.GetShippingDiscountTotal(),
                ShippingTotal = totals.ShippingTotal,
                HandlingTotal = totals.HandlingTotal,
                TaxTotal = totals.TaxTotal,
                CartTotal = totals.Total,
                Shipments = order.Forms.SelectMany(x => x.Shipments).Select(x => new ShipmentConfirmationViewModel
                {
                    Address = _addressBookService.ConvertToModel(x.ShippingAddress),
                    LineItems = x.LineItems,
                    ShipmentCost = x.GetShippingCost(_marketService.GetMarket(order.MarketId), order.Currency),
                    DiscountPrice = x.GetShipmentDiscountPrice(order.Currency),
                    ShippingItemsTotal = x.GetShippingItemsTotal(order.Currency),
                    ShippingMethodName = x.ShippingMethodName,
                })
            };
        }
    }
}
