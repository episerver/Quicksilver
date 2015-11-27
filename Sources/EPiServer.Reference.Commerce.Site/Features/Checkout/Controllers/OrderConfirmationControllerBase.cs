using EPiServer.Core;
using EPiServer.Editor;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Models;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Web.Mvc;
using EPiServer.Web.Mvc.Html;
using Mediachase.Commerce.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Controllers
{
    public abstract class OrderConfirmationControllerBase<T> : PageController<T> where T : PageData
    {
        protected readonly ConfirmationService _confirmationService;
        private readonly AddressBookService _addressBookService;
        protected readonly CustomerContextFacade _customerContext;

        protected OrderConfirmationControllerBase(ConfirmationService confirmationService, AddressBookService addressBookService, CustomerContextFacade customerContextFacade)
        {
            _confirmationService = confirmationService;
            _addressBookService = addressBookService;
            _customerContext = customerContextFacade;
        }

        protected OrderConfirmationViewModel<T> CreateViewModel(T currentPage, PurchaseOrder order)
        {
            var hasOrder = order != null;

            if (!hasOrder)
            {
                return new OrderConfirmationViewModel<T> { CurrentPage = currentPage };
            }

            var form = order.OrderForms.First();

            OrderConfirmationViewModel<T> viewModel = new OrderConfirmationViewModel<T>
            {
                CurrentPage = currentPage,
                HasOrder = hasOrder,
                OrderId = order.TrackingNumber,
                Created = order.Created,
                Items = form.LineItems,
                BillingAddress = new Address(),
                ShippingAddresses = new List<Address>(),
                ContactId = _customerContext.CurrentContactId,
                Payments = form.Payments,
                GroupId = order.OrderGroupId,
                OrderLevelDiscountTotal = order.ToMoney(form.LineItems.Sum(x=>x.OrderLevelDiscountAmount)),
                ShippingSubTotal = order.ToMoney(form.Shipments.Sum(s => s.ShippingSubTotal)),
                ShippingDiscountTotal = order.ToMoney(form.Shipments.Sum(s => s.ShippingDiscountAmount)),
                ShippingTotal = order.ToMoney(form.ShippingTotal),
                HandlingTotal = order.ToMoney(form.HandlingTotal),
                TaxTotal = order.ToMoney(form.TaxTotal),
                CartTotal = order.ToMoney(form.Total)
            };

            // Identify the id for all shipping addresses.
            IEnumerable<string> shippingAddressIdCollection = order.OrderForms.SelectMany(x => x.Shipments).Select(s => s.ShippingAddressId);

            // Map the billing address using the billing id of the order form.
            _addressBookService.MapOrderAddressToModel(viewModel.BillingAddress, order.OrderAddresses.Single(x => x.Name == form.BillingAddressId));

            // Map the remaining addresses as shipping addresses.
            foreach (OrderAddress orderAddress in order.OrderAddresses.Where(x => shippingAddressIdCollection.Contains(x.Name)))
            {
                ShippingAddress shippingAddress = new ShippingAddress();
                _addressBookService.MapOrderAddressToModel(shippingAddress, orderAddress);
                viewModel.ShippingAddresses.Add(shippingAddress);
            }

            return viewModel;
        }
    }
}
