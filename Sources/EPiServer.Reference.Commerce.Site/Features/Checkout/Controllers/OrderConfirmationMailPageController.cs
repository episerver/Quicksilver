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
    public class OrderConfirmationMailPageController : OrderConfirmationControllerBase<OrderConfirmationMailPage>
    {
        public OrderConfirmationMailPageController(
            ConfirmationService confirmationService, 
            AddressBookService addressBookService, 
            CustomerContextFacade customerContextFacade, 
            IOrderGroupTotalsCalculator orderGroupTotalsCalculator)
            : base(confirmationService, addressBookService, customerContextFacade, orderGroupTotalsCalculator)
        {
        }

        [HttpGet]
        public ActionResult Index(OrderConfirmationMailPage currentPage, int? orderNumber)
        {
            IPurchaseOrder order;
            if (PageEditing.PageIsInEditMode)
            {
                order = _confirmationService.CreateFakePurchaseOrder();
            }
            else
            {
                order = _confirmationService.GetOrder(orderNumber.Value);
                if (order == null)
                {
                    return Redirect(Url.ContentUrl(ContentReference.StartPage));
                }
            }
            
            var viewModel = CreateViewModel(currentPage, order);

            return View(viewModel);
        }
    }
}