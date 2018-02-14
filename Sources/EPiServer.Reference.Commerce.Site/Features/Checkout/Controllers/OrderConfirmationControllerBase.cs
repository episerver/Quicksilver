using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Controllers
{
    public abstract class OrderConfirmationControllerBase<T> : PageController<T> where T : PageData
    {
        protected readonly ConfirmationService ConfirmationService;
        protected readonly CustomerContextFacade CustomerContext;
        private readonly AddressBookService _addressBookService;
        private readonly IOrderGroupTotalsCalculator _orderGroupTotalsCalculator;
       
        protected OrderConfirmationControllerBase(
            ConfirmationService confirmationService,
            AddressBookService addressBookService,
            CustomerContextFacade customerContextFacade,
            IOrderGroupTotalsCalculator orderGroupTotalsCalculator)
        {
            ConfirmationService = confirmationService;
            _addressBookService = addressBookService;
            CustomerContext = customerContextFacade;
            _orderGroupTotalsCalculator = orderGroupTotalsCalculator;
        }

        protected OrderConfirmationViewModel<T> CreateViewModel(T currentPage, IPurchaseOrder order)
        {
            if (order == null)
            {
                return new OrderConfirmationViewModel<T> { CurrentPage = currentPage };
            }

            var totals = _orderGroupTotalsCalculator.GetTotals(order);

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
                OrderLevelDiscountTotal = order.GetOrderDiscountTotal(order.Currency),
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
                    ShipmentCost = x.GetShippingCost(order.Market, order.Currency),
                    DiscountPrice = x.GetShipmentDiscountPrice(order.Currency),
                    ShippingItemsTotal = x.GetShippingItemsTotal(order.Currency),
                    ShippingMethodName = x.ShippingMethodName,
                })
            };
        }
    }
}
