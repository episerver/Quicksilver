using System;
using System.Web.Mvc;
using EPiServer.Editor;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Models;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Controllers
{
    public class OrderConfirmationMailPageController : PageController<OrderConfirmationMailPage>
    {
        private readonly ConfirmationService _confirmationService;

        public OrderConfirmationMailPageController(ConfirmationService confirmationService)
        {
            _confirmationService = confirmationService;
        }

        [HttpGet]
        public ActionResult Index(OrderConfirmationMailPage currentPage, Guid? contactId = null, int orderNumber = 0)
        {
            var order = _confirmationService.GetOrder(orderNumber, PageEditing.PageIsInEditMode);
            var model = new OrderConfirmationMailViewModel
                {
                    CurrentPage = currentPage,
                    Order = order
                };
            if (order == null)
            {
                if (PageEditing.PageIsInEditMode)
                {
                    return View("Empty", model);
                }
                else
                {
                    return HttpNotFound();
                }
            }
            return View(model);
        }
    }
}