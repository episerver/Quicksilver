using EPiServer.Editor;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Models;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Market;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Web.Mvc;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;
using System;
using System.Linq;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Controllers
{
    public class OrderConfirmationController : PageController<OrderConfirmationPage>
    {
        private readonly ConfirmationService _confirmationService;
        private readonly CurrencyService _currencyService;

        public OrderConfirmationController(ConfirmationService confirmationService, CurrencyService currencyService)
        {
            _confirmationService = confirmationService;
            _currencyService = currencyService;
        }

        [HttpGet]
        public ActionResult Index(OrderConfirmationPage currentPage, Guid? contactId = null, int orderNumber = 0)
        {
            var order = _confirmationService.GetOrder(orderNumber, PageEditing.PageIsInEditMode);
            if (order == null && !PageEditing.PageIsInEditMode)
            {
                return HttpNotFound();
            }
            var model = CreateViewModel(currentPage, order);
            
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
                TotalPrice = order.ToMoney(form.LineItems.Sum(x => x.ExtendedPrice))
            };
        }
    }
}