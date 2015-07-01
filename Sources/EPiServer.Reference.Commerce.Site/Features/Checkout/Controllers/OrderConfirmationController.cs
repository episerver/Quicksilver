using EPiServer.Core;
using EPiServer.Editor;
using EPiServer.Reference.Commerce.Site.Features.AddressBook;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Models;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Web.Mvc;
using EPiServer.Web.Mvc.Html;
using Mediachase.Commerce.Customers;
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

        public OrderConfirmationController(ConfirmationService confirmationService, AddressBookService addressBookService)
        {
            _confirmationService = confirmationService;
            _addressBookService = addressBookService;
        }

        /// <summary>
        /// Renders the order confirmation view to the customer.
        /// </summary>
        /// <param name="currentPage">An instance of an OrderConfirmationPage.</param>
        /// <param name="notificationMessage">Any additional message that is vital to present to the customer.</param>
        /// <param name="contactId">The id of the contact if available.</param>
        /// <param name="orderNumber">The id of the related order.</param>
        /// <returns>The order confirmation view.</returns>
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

        private OrderConfirmationViewModel CreateViewModel(OrderConfirmationPage currentPage, PurchaseOrder order)
        {
            var hasOrder = order != null;

            if (!hasOrder)
            {
                return new OrderConfirmationViewModel();
            }

            var form = order.OrderForms.First();
            Dictionary<int, decimal> itemPrices = new Dictionary<int, decimal>();

            foreach (LineItem lineItem in form.LineItems)
            {
                itemPrices[lineItem.Id] = lineItem.Discounts.Count > 0 ? Math.Round(((lineItem.PlacedPrice * lineItem.Quantity) - lineItem.Discounts.Cast<LineItemDiscount>().Sum(x => x.DiscountValue)) / lineItem.Quantity, 2)
                                                                                        : lineItem.PlacedPrice;
            }

            OrderConfirmationViewModel viewModel = new OrderConfirmationViewModel
            {
                CurrentPage = currentPage,
                HasOrder = hasOrder,
                Created = order.Created,
                Items = form.LineItems,
                BillingAddress = new Address(),
                ShippingAddresses = new List<Address>(),
                ContactId = CustomerContext.Current.CurrentContactId,
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