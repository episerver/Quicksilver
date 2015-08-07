using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Globalization;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Models;
using EPiServer.Reference.Commerce.Site.Features.OrderHistory.Models;
using EPiServer.Reference.Commerce.Site.Features.OrderHistory.Pages;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Web.Mvc;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Orders;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.OrderHistory.Controllers
{
    [Authorize]
    public class OrderHistoryController : PageController<OrderHistoryPage>
    {
        private readonly IContentLoader _contentLoader;
        private readonly ReferenceConverter _referenceConverter;
        private readonly CultureInfo _preferredCulture;
        private readonly CustomerContextFacade _customerContext;
        private readonly IAddressBookService _addressBookService;

        public OrderHistoryController(IContentLoader contentLoader, ReferenceConverter referenceConverter, CustomerContextFacade customerContextFacade, IAddressBookService addressBookService)
        {
            _contentLoader = contentLoader;
            _referenceConverter = referenceConverter;
            _preferredCulture = ContentLanguage.PreferredCulture;
            _customerContext = customerContextFacade;
            _addressBookService = addressBookService;
        }

        [HttpGet]
        public ActionResult Index(OrderHistoryPage currentPage)
        {
            var purchaseOrders = OrderContext.Current.GetPurchaseOrders(_customerContext.CurrentContactId)
                                             .OrderByDescending(x => x.Created)
                                             .ToList();

            var lineItems = purchaseOrders.SelectMany(x => x.OrderForms.Any() ? x.OrderForms.First().LineItems.ToList() : new List<LineItem>());
            var variations = _contentLoader.GetItems(lineItems.Select(x => _referenceConverter.GetContentLink(x.Code)).ToList(),
                                                     _preferredCulture).OfType<VariationContent>();

            var orders = OrderContext.Current.GetPurchaseOrders(_customerContext.CurrentContactId);
            var viewModel = new OrderHistoryViewModel
            {
                CurrentPage = currentPage,
                Orders = new List<Order>()
            };

            foreach (var item in orders.OrderByDescending(x => x.Created))
            {
                // Assume there is only one form per purchase.
                OrderForm form = item.OrderForms[0];

                Order order = new Order
                {
                    PurchaseOrder = item,
                    Items = form.LineItems.Select(lineItem => new OrderHistoryItem
                    {
                        LineItem = lineItem,
                        Variation = variations.FirstOrDefault(y => y.Code == lineItem.Code)
                    }),
                    BillingAddress = new Address(),
                    ShippingAddresses = new List<Address>()
                };

                // Identify the id for all shipping addresses.
                IEnumerable<string> shippingAddressIdCollection = item.OrderForms.SelectMany(x => x.Shipments).Select(s => s.ShippingAddressId);

                // Map the billing address using the billing id of the order form.
                _addressBookService.MapOrderAddressToModel(order.BillingAddress, item.OrderAddresses.Single(x => x.Name == form.BillingAddressId));

                // Map the remaining addresses as shipping addresses.
                foreach (OrderAddress orderAddress in item.OrderAddresses.Where(x => shippingAddressIdCollection.Contains(x.Name)))
                {
                    ShippingAddress shippingAddress = new ShippingAddress();
                    _addressBookService.MapOrderAddressToModel(shippingAddress, orderAddress);
                    order.ShippingAddresses.Add(shippingAddress);
                }

                viewModel.Orders.Add(order);
            }

            return View(viewModel);
        }
    }
}