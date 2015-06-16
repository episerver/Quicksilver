using System;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.Editor;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Models;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Web.Mvc;
using EPiServer.Web.Mvc.Html;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Controllers
{
    public class OrderConfirmationController : PageController<OrderConfirmationPage>
    {
        private readonly ConfirmationService _confirmationService;

        public OrderConfirmationController(ConfirmationService confirmationService)
        {
            _confirmationService = confirmationService;
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
            return new OrderConfirmationViewModel
            {
                CurrentPage = currentPage,
                HasOrder = hasOrder,
                Created = order.Created,
                Items = form.LineItems,
                Address = order.OrderAddresses.First(),
                ContactId = CustomerContext.Current.CurrentContactId,
                Payment = (CreditCardPayment)form.Payments.First(),
                GroupId = order.OrderGroupId,
                TotalPrice = order.ToMoney(form.Total)
            };
        }
    }
}