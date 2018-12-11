using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Shared.Identity;
using EPiServer.Reference.Commerce.Site.Features.Login.Services;
using EPiServer.Reference.Commerce.Site.Features.Registration.Blocks;
using EPiServer.Reference.Commerce.Site.Features.Registration.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Controllers;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Registration.Controllers
{
    public class OrderConfirmationRegistrationBlockController : IdentityControllerBase<OrderConfirmationRegistrationBlock>
    {
        private readonly CustomerContextFacade _customerContext;
        private readonly IOrderRepository _orderRepository;

        public OrderConfirmationRegistrationBlockController(
            ApplicationSignInManager<SiteUser> applicationSignInManager, 
            ApplicationUserManager<SiteUser> applicationUserManager, 
            UserService userService, 
            CustomerContextFacade customerContextFacade, 
            IOrderRepository orderRepository)
            : base(applicationSignInManager, applicationUserManager, userService)
        {
            _customerContext = customerContextFacade;
            _orderRepository = orderRepository;
        }

        [HttpGet]
        public ActionResult Index(OrderConfirmationRegistrationBlock currentBlock)
        {
            OrderConfirmationRegistrationModel model = null;
            var orderNumber = ControllerContext.ParentActionViewContext.ViewData["OrderNumber"] as int? ?? -1;
            var contactId = ControllerContext.ParentActionViewContext.ViewData["ContactId"] as Guid? ?? Guid.Empty;
            var order = _orderRepository.Load<IPurchaseOrder>(orderNumber);

            if (order == null || _customerContext.GetContactById(order.CustomerId) != null)
            {
                return null;
            }

            SiteUser user = UserService.GetUser(order.GetFirstForm().Payments.First().BillingAddress.Email);

            if (user != null)
            {
                model = new OrderConfirmationRegistrationModel
                {
                    CurrentBlock = currentBlock,
                    FormModel = new OrderConfirmationRegistrationFormModel
                    {
                        OrderNumber = orderNumber,
                        ContactId = contactId
                    }
                };

                return PartialView(model.FormModel);
            }

            model = new OrderConfirmationRegistrationModel
            {
                CurrentBlock = currentBlock,
                FormModel = new OrderConfirmationRegistrationFormModel                        
                {
                    OrderNumber = orderNumber,
                    ContactId = contactId
                }
            };

            return PartialView("NewCustomer", model.FormModel);
        }

        [HttpPost]
        public async Task<ActionResult> Register(OrderConfirmationRegistrationBlock currentBlock, OrderConfirmationRegistrationFormModel viewModel)
        {
            var purchaseOrder = _orderRepository.Load<IPurchaseOrder>(viewModel.OrderNumber); 
            
            var model = new OrderConfirmationRegistrationModel
            {
                CurrentBlock = currentBlock,
                FormModel = viewModel
            };

            if (purchaseOrder == null)
            {
                ModelState.AddModelError("Password2", "Something went wrong");
            }

            if (!ModelState.IsValid || purchaseOrder == null)
            {
                return PartialView("NewCustomer", model.FormModel);
            }

            ContactIdentityResult registration = await UserService.RegisterAccount(new SiteUser(purchaseOrder)
            { 
                Password = viewModel.Password, 
                RegistrationSource = "Order confirmation page",
                IsApproved = true
            });

            if (registration.Result.Succeeded)
            {
                if (registration.Contact.PrimaryKeyId.HasValue)
                {
                    purchaseOrder.CustomerId = registration.Contact.PrimaryKeyId.Value;
                    
                    _orderRepository.Save(purchaseOrder);
                }
                return PartialView("Complete", registration.Contact.Email);
            }

            if (registration.Result.Errors.Any())
            {
                registration.Result.Errors.ToList().ForEach(x => ModelState.AddModelError("Password2", x));
                return PartialView("NewCustomer", model.FormModel);
            }

            return PartialView("Index", model.FormModel);
        }

        [HttpPost]
        public ActionResult Assign(OrderConfirmationRegistrationFormModelBase viewModel)
        {
            var purchaseOrder = _orderRepository.Load<IPurchaseOrder>(viewModel.OrderNumber);
            var contact = UserService.GetCustomerContact(purchaseOrder.GetFirstForm().Payments.First().BillingAddress.Email);
            if (contact.PrimaryKeyId.HasValue)
            {
                purchaseOrder.CustomerId = contact.PrimaryKeyId.Value;
                _orderRepository.Save(purchaseOrder);
            }
            return PartialView("Complete2");
        }
    }
}