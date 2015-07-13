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
    public class OrderConfirmationController : PageController<OrderConfirmationPage>
    {
        private readonly ConfirmationService _confirmationService;
        private readonly AddressBookService _addressBookService;
        private readonly CustomerContextFacade _customerContext;

        public OrderConfirmationController(ConfirmationService confirmationService, AddressBookService addressBookService, CustomerContextFacade customerContextFacade)
        {
            _confirmationService = confirmationService;
            _addressBookService = addressBookService;
            _customerContext = customerContextFacade;
        }

        [HttpGet]
        public ActionResult Index(OrderConfirmationPage currentPage, string notificationMessage, Guid? contactId = null, int orderNumber = 0)
        {
            var order = _confirmationService.GetOrder(orderNumber, PageEditing.PageIsInEditMode);
            if (order == null && !PageEditing.PageIsInEditMode)
            {
                return Redirect(Url.ContentUrl(ContentReference.StartPage));
            }

            var model = CreateViewModel(currentPage, order);
            model.NotificationMessage = notificationMessage;

            return View(model);
        }

        private OrderConfirmationViewModel<OrderConfirmationPage> CreateViewModel(OrderConfirmationPage currentPage, PurchaseOrder order)
        {
            var hasOrder = order != null;

            if (!hasOrder)
            {
                return new OrderConfirmationViewModel<OrderConfirmationPage> { CurrentPage = currentPage };
            }

            var form = order.OrderForms.First();
            Dictionary<int, decimal> itemPrices = new Dictionary<int, decimal>();

            foreach (LineItem lineItem in form.LineItems)
            {
                itemPrices[lineItem.Id] = lineItem.Discounts.Count > 0 ? Math.Round(((lineItem.PlacedPrice * lineItem.Quantity) - lineItem.Discounts.Cast<LineItemDiscount>().Sum(x => x.DiscountValue)) / lineItem.Quantity, 2)
                                                                                        : lineItem.PlacedPrice;
            }

            OrderConfirmationViewModel<OrderConfirmationPage> viewModel = new OrderConfirmationViewModel<OrderConfirmationPage>
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
                ShippingTotal = order.ToMoney(form.ShippingTotal),
                TotalPrice = order.ToMoney(form.Total),
                ItemPrices = itemPrices
            };
            
            // We will make the assumption that the first order is the billing address and the rest is shipping addresses.
            _addressBookService.MapOrderAddressToModel(viewModel.BillingAddress, order.OrderAddresses[0]);

            for (int index = 1; index < order.OrderAddresses.Count; index++)
            {
                ShippingAddress shippingAddress = new ShippingAddress();
                _addressBookService.MapOrderAddressToModel(shippingAddress, order.OrderAddresses[index]);
                viewModel.ShippingAddresses.Add(shippingAddress);
            }

            return viewModel;
            
        }
    }
}