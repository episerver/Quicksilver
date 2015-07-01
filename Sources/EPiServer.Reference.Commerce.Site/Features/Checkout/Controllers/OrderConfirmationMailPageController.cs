using System;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Editor;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Models;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Web.Mvc;
using System.Collections.Generic;
using Mediachase.Commerce.Orders;

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
            var form = order.OrderForms.First();
            Dictionary<int, decimal> itemPrices = new Dictionary<int, decimal>();
            foreach (LineItem lineItem in form.LineItems)
            {
                itemPrices[lineItem.Id] = lineItem.Discounts.Count > 0 ? Math.Round(((lineItem.PlacedPrice * lineItem.Quantity) - lineItem.Discounts.Cast<LineItemDiscount>().Sum(x => x.DiscountValue)) / lineItem.Quantity, 2)
                                                                                        : lineItem.PlacedPrice;
            }
            var model = new OrderConfirmationMailViewModel
                {
                    CurrentPage = currentPage,
                    Order = order,
                    ItemPrices = itemPrices
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