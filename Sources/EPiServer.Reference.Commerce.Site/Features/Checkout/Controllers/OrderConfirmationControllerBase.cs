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

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Controllers
{
    public abstract class OrderConfirmationControllerBase<T> : PageController<T> where T : PageData
    {
        protected readonly ConfirmationService _confirmationService;
        private readonly AddressBookService _addressBookService;
        protected readonly CustomerContextFacade _customerContext;
        private readonly IOrderGroupTotalsCalculator _orderGroupTotalsCalculator;

        protected OrderConfirmationControllerBase(
            ConfirmationService confirmationService, 
            AddressBookService addressBookService, 
            CustomerContextFacade customerContextFacade,
            IOrderGroupTotalsCalculator orderGroupTotalsCalculator)
        {
            _confirmationService = confirmationService;
            _addressBookService = addressBookService;
            _customerContext = customerContextFacade;
            _orderGroupTotalsCalculator = orderGroupTotalsCalculator;
        }

        protected OrderConfirmationViewModel<T> CreateViewModel(T currentPage, IPurchaseOrder order)
        {
            var hasOrder = order != null;

            if (!hasOrder)
            {
                return new OrderConfirmationViewModel<T> { CurrentPage = currentPage };
            }
            
            var lineItems = order.GetFirstForm().Shipments.SelectMany(x => x.LineItems);
            var totals = _orderGroupTotalsCalculator.GetTotals(order);
            
            var viewModel = new OrderConfirmationViewModel<T>
            {
                Currency = order.Currency,
                CurrentPage = currentPage,
                HasOrder = hasOrder,
                OrderId = order.OrderNumber,
                Created = order.Created,
                Items = lineItems,
                BillingAddress = new AddressModel(),
                ShippingAddresses = new List<AddressModel>(),
                ContactId = _customerContext.CurrentContactId,
                Payments = order.GetFirstForm().Payments,
                OrderGroupId = order.OrderLink.OrderGroupId,
                OrderLevelDiscountTotal = order.GetOrderDiscountTotal(order.Currency),
                ShippingSubTotal = order.GetShippingSubTotal(),
                ShippingDiscountTotal = order.GetShippingDiscountTotal(), 
                ShippingTotal = totals.ShippingTotal,
                HandlingTotal = totals.HandlingTotal, 
                TaxTotal = totals.TaxTotal,
                CartTotal = totals.Total
            };
            
            var billingAddress = order.GetFirstForm().Payments.First().BillingAddress; 
            
            // Map the billing address using the billing id of the order form.
            _addressBookService.MapToModel(billingAddress, viewModel.BillingAddress);

            // Map the remaining addresses as shipping addresses.
            foreach (var orderAddress in order.Forms.SelectMany(x => x.Shipments).Select(s => s.ShippingAddress))
            {
                var shippingAddress = new AddressModel();
                _addressBookService.MapToModel(orderAddress, shippingAddress);
                viewModel.ShippingAddresses.Add(shippingAddress);
            }

            return viewModel;
        }
    }
}
