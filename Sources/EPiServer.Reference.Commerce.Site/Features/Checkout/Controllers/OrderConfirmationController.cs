using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Editor;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Web.Mvc.Html;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Controllers
{
    public class OrderConfirmationController : OrderConfirmationControllerBase<OrderConfirmationPage>
    {
        public OrderConfirmationController(
            ConfirmationService confirmationService,
            AddressBookService addressBookService,
            CustomerContextFacade customerContextFacade,
            IOrderGroupTotalsCalculator orderGroupTotalsCalculator)
            : base(confirmationService, addressBookService, customerContextFacade, orderGroupTotalsCalculator)
        {
        }

        [HttpGet]
        public ActionResult Index(OrderConfirmationPage currentPage, string notificationMessage, int? orderNumber)
        {
            IPurchaseOrder order = null;
            if (PageEditing.PageIsInEditMode)
            {
                order = _confirmationService.CreateFakePurchaseOrder();
            }
            else if (orderNumber.HasValue)
            {
                order = _confirmationService.GetOrder(orderNumber.Value);
            }

            if (order != null && order.CustomerId == _customerContext.CurrentContactId)
            {
                var viewModel = CreateViewModel(currentPage, order);
                viewModel.NotificationMessage = notificationMessage;

                return View(viewModel);
            }

            return Redirect(Url.ContentUrl(ContentReference.StartPage));
        }
    }
}