using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.OrderHistory.Pages;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.OrderHistory.ViewModels;

namespace EPiServer.Reference.Commerce.Site.Features.OrderHistory.Controllers
{
    [Authorize]
    public class OrderHistoryController : PageController<OrderHistoryPage>
    {
        private readonly CustomerContextFacade _customerContext;
        private readonly IAddressBookService _addressBookService;
        private readonly IOrderRepository _orderRepository;

        public OrderHistoryController(CustomerContextFacade customerContextFacade, IAddressBookService addressBookService, IOrderRepository orderRepository)
        {
            _customerContext = customerContextFacade;
            _addressBookService = addressBookService;
            _orderRepository = orderRepository;
        }

        [HttpGet]
        public ActionResult Index(OrderHistoryPage currentPage)
        {
            var purchaseOrders = _orderRepository.Load<IPurchaseOrder>(_customerContext.CurrentContactId)
                                             .OrderByDescending(x => x.Created)
                                             .ToList();

            var viewModel = new OrderHistoryViewModel
            {
                CurrentPage = currentPage,
                Orders = new List<OrderViewModel>()
            };

            foreach (var purchaseOrder in purchaseOrders)
            {
                // Assume there is only one form per purchase.
                var form = purchaseOrder.GetFirstForm();
				var billingAddress = new AddressModel();
                var payment = form.Payments.FirstOrDefault();
				if (payment != null)
				{
					billingAddress = _addressBookService.ConvertToModel(payment.BillingAddress);
				}
                var orderViewModel = new OrderViewModel
                {
                    PurchaseOrder = purchaseOrder,
                    Items = form.GetAllLineItems().Select(lineItem => new OrderHistoryItemViewModel
                    {
                        LineItem = lineItem,
                    }).GroupBy(x => x.LineItem.Code).Select(group => group.First()),
                    BillingAddress = billingAddress,
                    ShippingAddresses = new List<AddressModel>()
                };

                foreach (var orderAddress in purchaseOrder.Forms.SelectMany(x => x.Shipments).Select(s => s.ShippingAddress))
                {
                    var shippingAddress = _addressBookService.ConvertToModel(orderAddress);
                    orderViewModel.ShippingAddresses.Add(shippingAddress);
                }

                viewModel.Orders.Add(orderViewModel);
            }

            return View(viewModel);
        }
    }
}