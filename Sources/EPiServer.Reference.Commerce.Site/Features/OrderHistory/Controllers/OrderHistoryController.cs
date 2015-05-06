using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.Reference.Commerce.Site.Features.OrderHistory.Models;
using EPiServer.Reference.Commerce.Site.Features.OrderHistory.Page;
using EPiServer.Web.Mvc;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;

namespace EPiServer.Reference.Commerce.Site.Features.OrderHistory.Controllers
{
    [Authorize]
    public class OrderHistoryController : PageController<OrderHistoryPage>
    {
        private readonly IContentLoader _contentLoader;
        private readonly ReferenceConverter _referenceConverter;
        private readonly CultureInfo _preferredCulture;

        public OrderHistoryController(IContentLoader contentLoader, ReferenceConverter referenceConverter)
        {
            _contentLoader = contentLoader;
            _referenceConverter = referenceConverter;
            _preferredCulture = ContentLanguage.PreferredCulture;
        }

        [HttpGet]
        public ActionResult Index(OrderHistoryPage currentPage)
        {
            var purchaseOrders = OrderContext.Current.GetPurchaseOrders(CustomerContext.Current.CurrentContactId)
                                             .OrderByDescending(x => x.Created)
                                             .ToList();
            var lineItems = purchaseOrders.SelectMany(x => x.OrderForms.Any() ? x.OrderForms.First().LineItems.ToList() : new List<LineItem>());
            var variations = _contentLoader.GetItems(lineItems.Select(x => _referenceConverter.GetContentLink(x.Code)).ToList(),
                                                     _preferredCulture).OfType<VariationContent>();
            var model = new OrderHistoryViewModel
                {
                    CurrentPage = currentPage,
                    Orders = OrderContext.Current.GetPurchaseOrders(CustomerContext.Current.CurrentContactId)
                                         .OrderByDescending(x => x.Created).Select(x => new Order
                                             {
                                                 PurchaseOrder = x,
                                                 Items = x.OrderForms.First().LineItems.Select(lineItem => new OrderHistoryItem
                                                     {
                                                         LineItem = lineItem,
                                                         Variation = variations.FirstOrDefault(y => y.Code == lineItem.Code)
                                                     })
                                             }).ToList()
                };

            return View(model);
        }
    }
}