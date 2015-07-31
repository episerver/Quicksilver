using EPiServer.Core;
using EPiServer.Editor;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Models;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Web.Mvc.Html;
using Mediachase.Commerce.Orders;
using System;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Controllers
{
    public class OrderConfirmationMailPageController : OrderConfirmationControllerBase<OrderConfirmationMailPage>
    {
        public OrderConfirmationMailPageController(ConfirmationService confirmationService, AddressBookService addressBookService, CustomerContextFacade customerContextFacade)
            : base(confirmationService, addressBookService, customerContextFacade)
        {
        }

        [HttpGet]
        public ActionResult Index(OrderConfirmationMailPage currentPage, Guid? contactId = null, int orderNumber = 0)
        {
            PurchaseOrder order = _confirmationService.GetOrder(orderNumber, PageEditing.PageIsInEditMode);

            if (order == null && !PageEditing.PageIsInEditMode)
            {
                return Redirect(Url.ContentUrl(ContentReference.StartPage));
            }

            OrderConfirmationViewModel<OrderConfirmationMailPage> model = CreateViewModel(currentPage, order);

            return View(model);
        }
    }
}