using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;
using EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Web.Mvc;
using EPiServer.Web.Mvc.Html;
using EPiServer.Web.Routing;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Controllers
{
    public class CheckoutController : PageController<CheckoutPage>
    {
        private readonly IContentRepository _contentRepository;
        private readonly IMailService _mailService;
        private readonly LocalizationService _localizationService;
        private readonly ICurrencyService _currencyService;
        private readonly ControllerExceptionHandler _controllerExceptionHandler;
        private readonly CustomerContextFacade _customerContext;
        private readonly CheckoutViewModelFactory _checkoutViewModelFactory;
        private readonly OrderSummaryViewModelFactory _orderSummaryViewModelFactory;
        private readonly IOrderGroupCalculator _orderGroupCalculator;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly IOrderRepository _orderRepository;
        private readonly IPromotionEngine _promotionEngine;
        private readonly ICartService _cartService;
        private readonly IAddressBookService _addressBookService;
        private readonly IOrderFactory _orderFactory;
        private ICart _cart;

        public CheckoutController(IContentRepository contentRepository,
            IMailService mailService,
            LocalizationService localizationService,
            ICurrencyService currencyService,
            ControllerExceptionHandler controllerExceptionHandler,
            CustomerContextFacade customerContextFacade,
            IOrderRepository orderRepository,
            CheckoutViewModelFactory checkoutViewModelFactory,
            IOrderGroupCalculator orderGroupCalculator,
            IPaymentProcessor paymentProcessor,
            IPromotionEngine promotionEngine,
            ICartService cartService,
            IAddressBookService addressBookService,
            OrderSummaryViewModelFactory orderSummaryViewModelFactory,
            IOrderFactory orderFactory)
        {
            _contentRepository = contentRepository;
            _mailService = mailService;
            _localizationService = localizationService;
            _currencyService = currencyService;
            _controllerExceptionHandler = controllerExceptionHandler;
            _customerContext = customerContextFacade;
            _orderRepository = orderRepository;
            _checkoutViewModelFactory = checkoutViewModelFactory;
            _orderGroupCalculator = orderGroupCalculator;
            _paymentProcessor = paymentProcessor;
            _promotionEngine = promotionEngine;
            _cartService = cartService;
            _addressBookService = addressBookService;
            _orderSummaryViewModelFactory = orderSummaryViewModelFactory;
            _orderFactory = orderFactory;
        }

        [HttpGet]
        [OutputCache(Duration = 0, NoStore = true)]
        public ActionResult Index(CheckoutPage currentPage)
        {
            if (CartIsNullOrEmpty())
            {
                return View("EmptyCart");
            }

            bool cartChanged = false;

            if (Cart.Currency == null)
            {
                Cart.Currency = _currencyService.GetCurrentCurrency();
                cartChanged = true;
            }

            var promotionsCount = Cart.GetFirstForm().Promotions.Count;
            Cart.ApplyDiscounts(_promotionEngine, new PromotionEngineSettings());
            if (Cart.GetFirstForm().Promotions.Count != promotionsCount)
            {
                cartChanged = true;
            }

            var viewModel = CreateCheckoutViewModel(currentPage);

            if (Cart.GetFirstForm().Shipments.Any(c => c.ShippingMethodId == Guid.Empty))
            {
                UpdateShippingMethodIds(Cart, viewModel.Shipments);
                cartChanged = true;
            }

            // If any default billing and shipping address then set to cart and save cart.
            if (viewModel.UseBillingAddressForShipment)
            {
                if (!string.IsNullOrEmpty(viewModel.BillingAddress.AddressId))
                {
                    Cart.GetFirstForm().Shipments.First().ShippingAddress = _addressBookService.ConvertToAddress(viewModel.BillingAddress);
                    cartChanged = true;
                }
            }
            else
            {
                if (viewModel.Shipments.Any(s => !string.IsNullOrEmpty(s.Address.AddressId)))
                {
                    SetShipmentAddresses(viewModel.Shipments);
                    cartChanged = true;
                }
            }

            if (cartChanged)
            {
                _orderRepository.Save(Cart);
            }

            return View(viewModel.ViewName, viewModel);
        }

        [HttpGet]
        public ActionResult SingleShipment(CheckoutPage currentPage)
        {
            if (Cart != null)
            {
                _cartService.MergeShipments(Cart);
                _orderRepository.Save(Cart);
            }

            return RedirectToAction("Index", new { node = currentPage.ContentLink });
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult Update(CheckoutPage currentPage, UpdateShippingMethodViewModel shipmentViewModel, IPaymentMethodViewModel<PaymentMethodBase> paymentViewModel)
        {
            ModelState.Clear();

            UpdateShippingMethodIds(Cart, shipmentViewModel.Shipments);
            _orderRepository.Save(Cart);

            var viewModel = CreateCheckoutViewModel(currentPage, paymentViewModel);

            return PartialView("Partial", viewModel);
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult ChangeAddress(UpdateAddressViewModel addressViewModel)
        {
            ModelState.Clear();

            var viewModel = ChangeShippingAddress(addressViewModel);
            var addressViewName = addressViewModel.ShippingAddressIndex > -1 ? "SingleShippingAddress" : "BillingAddress";

            if (viewModel.UseBillingAddressForShipment)
            {
                Cart.GetFirstForm().Shipments.First().ShippingAddress = _addressBookService.ConvertToAddress(viewModel.BillingAddress);
            }
            else
            {
                SetShipmentAddresses(viewModel.Shipments);
            }

            _orderRepository.Save(Cart);

            return PartialView(addressViewName, viewModel);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult OrderSummary()
        {
            var viewModel = _orderSummaryViewModelFactory.CreateOrderSummaryViewModel(Cart);
            return PartialView(viewModel);
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult AddCouponCode(CheckoutPage currentPage, string couponCode)
        {
            if (_cartService.AddCouponCode(Cart, couponCode))
            {
                _orderRepository.Save(Cart);
            }
            var viewModel = CreateCheckoutViewModel(currentPage);
            return View(viewModel.ViewName, viewModel);
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult RemoveCouponCode(CheckoutPage currentPage, string couponCode)
        {
            _cartService.RemoveCouponCode(Cart, couponCode);
            _orderRepository.Save(Cart);
            var viewModel = CreateCheckoutViewModel(currentPage);
            return View(viewModel.ViewName, viewModel);
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult Purchase(CheckoutViewModel checkoutViewModel, IPaymentMethodViewModel<PaymentMethodBase> paymentViewModel)
        {
            if (CartIsNullOrEmpty())
            {
                return Redirect(Url.ContentUrl(ContentReference.StartPage));
            }

            if (User.Identity.IsAuthenticated)
            {
                // load billing and shipping address from address book
                _addressBookService.LoadAddress(checkoutViewModel.BillingAddress);

                foreach (var shipment in checkoutViewModel.Shipments)
                {
                    _addressBookService.LoadAddress(shipment.Address);
                }

                // Because address is selected from drop down, so remove validation for fields
                foreach (var state in ModelState.Where(x => (x.Key.StartsWith("BillingAddress")) || x.Key.StartsWith("Shipments")).ToArray())
                {
                    ModelState.Remove(state);
                }
            }

            if (checkoutViewModel.UseBillingAddressForShipment)
            {
                // If only the billing address is of interest we need to remove any existing error related to the
                // other shipping addresses.
                if (!User.Identity.IsAuthenticated)
                {
                    foreach (var state in ModelState.Where(x => x.Key.StartsWith("Shipments")).ToArray())
                    {
                        ModelState.Remove(state);
                    }
                }

                checkoutViewModel.Shipments.Single().Address = checkoutViewModel.BillingAddress;
            }

            ValidateBillingAddress(checkoutViewModel);

            HandleValidationIssues(_cartService.ValidateCart(Cart));

            if (!ModelState.IsValid)
            {
                return View(checkoutViewModel, paymentViewModel);
            }

            // Since the payment property is marked with an exclude binding attribute in the CheckoutViewModel
            // it needs to be manually re-added again.
            checkoutViewModel.Payment = paymentViewModel;

            ValidateShippingAddress(checkoutViewModel);

            HandleValidationIssues(_cartService.RequestInventory(Cart));

            if (!ModelState.IsValid)
            {
                return View(checkoutViewModel, paymentViewModel);
            }

            UpdateAddressNames(checkoutViewModel);

            SetShipmentAddresses(checkoutViewModel.Shipments);

            CreatePayment(checkoutViewModel.Payment, checkoutViewModel.BillingAddress);

            var startpage = _contentRepository.Get<StartPage>(ContentReference.StartPage);
            var confirmationPage = _contentRepository.GetFirstChild<OrderConfirmationPage>(checkoutViewModel.CurrentPage.ContentLink);
            IPurchaseOrder purchaseOrder = null;

            try
            {
                purchaseOrder = PlaceOrder(checkoutViewModel);
            }
            catch (PaymentException)
            {
                ModelState.AddModelError("", _localizationService.GetString("/Checkout/Payment/Errors/ProcessingPaymentFailure"));
                return View(checkoutViewModel, paymentViewModel);
            }

            var queryCollection = new NameValueCollection
            {
                {"contactId", _customerContext.CurrentContactId.ToString()},
                {"orderNumber", purchaseOrder.OrderLink.OrderGroupId.ToString(CultureInfo.CurrentCulture)}
            };

            SendConfirmationEmail(checkoutViewModel.BillingAddress.Email, startpage.OrderConfirmationMail, confirmationPage.Language.Name, queryCollection);

            return Redirect(new UrlBuilder(confirmationPage.LinkURL) { QueryCollection = queryCollection }.ToString());
        }

        private void ValidateBillingAddress(CheckoutViewModel checkoutViewModel)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (string.IsNullOrEmpty(checkoutViewModel.BillingAddress.AddressId))
                {
                    ModelState.AddModelError("BillingAddress.AddressId", _localizationService.GetString("/Shared/Address/Form/Empty/BillingAddress"));
                }
            }
            else
            {
                if (string.IsNullOrEmpty(checkoutViewModel.BillingAddress.Email))
                {
                    ModelState.AddModelError("BillingAddress.Email", _localizationService.GetString("/Shared/Address/Form/Empty/Email"));
                }
            }
        }

        private void ValidateShippingAddress(CheckoutViewModel checkoutViewModel)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (checkoutViewModel.Shipments.Any(a => string.IsNullOrEmpty(a.Address.AddressId)))
                {
                    ModelState.AddModelError("ShippingAddress.AddressId", _localizationService.GetString("/Shared/Address/Form/Empty/ShippingAddress"));
                }
            }
        }

        private void HandleValidationIssues(Dictionary<ILineItem, List<ValidationIssue>> validationIssueCollections)
        {
            foreach (var validationIssue in validationIssueCollections)
            {
                foreach (var issue in validationIssue.Value)
                {
                    switch (issue)
                    {
                        case ValidationIssue.None:
                            break;

                        case ValidationIssue.CannotProcessDueToMissingOrderStatus:
                            ModelState.AddModelError("", string.Format(_localizationService.GetString("/Checkout/Payment/Errors/CannotProcessDueToMissingOrderStatus"), validationIssue.Key.Code));
                            break;

                        case ValidationIssue.RemovedDueToCodeMissing:
                        case ValidationIssue.RemovedDueToNotAvailableInMarket:
                        case ValidationIssue.RemovedDueToInactiveWarehouse:
                        case ValidationIssue.RemovedDueToMissingInventoryInformation:
                        case ValidationIssue.RemovedDueToUnavailableCatalog:
                        case ValidationIssue.RemovedDueToUnavailableItem:
                            ModelState.AddModelError("", string.Format(_localizationService.GetString("/Checkout/Payment/Errors/RemovedDueToUnavailableItem"), validationIssue.Key.Code));
                            break;

                        case ValidationIssue.RemovedDueToInsufficientQuantityInInventory:
                            ModelState.AddModelError("", string.Format(_localizationService.GetString("/Checkout/Payment/Errors/RemovedDueToInsufficientQuantityInInventory"), validationIssue.Key.Code));
                            break;

                        case ValidationIssue.RemovedDueToInvalidPrice:
                            ModelState.AddModelError("", string.Format(_localizationService.GetString("/Checkout/Payment/Errors/RemovedDueToInvalidPrice"), validationIssue.Key.Code));
                            break;

                        case ValidationIssue.AdjustedQuantityByMinQuantity:
                        case ValidationIssue.AdjustedQuantityByMaxQuantity:
                        case ValidationIssue.AdjustedQuantityByBackorderQuantity:
                        case ValidationIssue.AdjustedQuantityByPreorderQuantity:
                        case ValidationIssue.AdjustedQuantityByAvailableQuantity:
                            ModelState.AddModelError("", string.Format(_localizationService.GetString("/Checkout/Payment/Errors/AdjustedQuantity"), validationIssue.Key.Code));
                            break;

                        case ValidationIssue.PlacedPricedChanged:
                            ModelState.AddModelError("", string.Format(_localizationService.GetString("/Checkout/Payment/Errors/PlacedPricedChanged"), validationIssue.Key.Code));
                            break;

                        default:
                            ModelState.AddModelError("", string.Format(_localizationService.GetString("/Checkout/Payment/Errors/PreProcessingFailure"), validationIssue.Key.Code));
                            break;
                    }
                }
            }
        }

        private IPurchaseOrder PlaceOrder(CheckoutViewModel checkoutViewModel)
        {
            Cart.ProcessPayments(_paymentProcessor, _orderGroupCalculator);
            var totalProcessedAmount = Cart.GetFirstForm().Payments.Where(x => x.Status.Equals(PaymentStatus.Processed.ToString())).Sum(x => x.Amount);

            if (totalProcessedAmount != Cart.GetTotal(_orderGroupCalculator).Amount)
            {
                throw new InvalidOperationException("Wrong amount");
            }

            var payment = Cart.GetFirstForm().Payments.First();
            checkoutViewModel.Payment.PaymentMethod.PostProcess(payment);

            var orderReference = _orderRepository.SaveAsPurchaseOrder(Cart);
            var purchaseOrder = _orderRepository.Load<IPurchaseOrder>(orderReference.OrderGroupId);
            _orderRepository.Delete(Cart.OrderLink);

            return purchaseOrder;
        }

        private void SendConfirmationEmail(string emailAddress, ContentReference confirmationEmail, string language, NameValueCollection queryCollection)
        {
            try
            {
                _mailService.Send(confirmationEmail, queryCollection, emailAddress, language);
            }
            catch (Exception)
            {
                // The purchase has been processed and the payment was successfully settled, but for some reason the e-mail
                // receipt could not be sent to the customer. Rollback is not possible so simple make sure to inform the
                // customer to print the confirmation page instead.
                queryCollection.Add("notificationMessage", string.Format(_localizationService.GetString("/OrderConfirmationMail/ErrorMessages/SmtpFailure"), emailAddress));

                // Todo: Log the error and raise an alert so that an administrator can look in to it.
            }
        }

        private void SetShipmentAddresses(IList<ShipmentViewModel> shipmentViewModels)
        {
            var shipments = Cart.GetFirstForm().Shipments;
            for (int index = 0; index < shipments.Count; index++)
            {
                shipments.ElementAt(index).ShippingAddress = _addressBookService.ConvertToAddress(shipmentViewModels[index].Address);
            }
        }

        private void CreatePayment(IPaymentMethodViewModel<PaymentMethodBase> paymentViewModel, AddressModel billingAddress)
        {
            IOrderAddress address = _addressBookService.ConvertToAddress(billingAddress);
            var total = Cart.GetTotal(_orderGroupCalculator);
            var payment = paymentViewModel.PaymentMethod.CreatePayment(total.Amount);
            Cart.AddPayment(payment, _orderFactory);
            payment.BillingAddress = address;
        }
                
        private ViewResult View(CheckoutViewModel checkoutViewModel, IPaymentMethodViewModel<PaymentMethodBase> paymentViewModel)
        {
            return View(checkoutViewModel.ViewName, CreateCheckoutViewModel(checkoutViewModel.CurrentPage, paymentViewModel));
        }

        private CheckoutViewModel CreateCheckoutViewModel(CheckoutPage currentPage, IPaymentMethodViewModel<PaymentMethodBase> paymentViewModel = null)
        {
            return _checkoutViewModelFactory.CreateCheckoutViewModel(Cart, currentPage, paymentViewModel);
        }

        public ActionResult OnPurchaseException(ExceptionContext filterContext)
        {
            var currentPage = filterContext.RequestContext.GetRoutedData<CheckoutPage>();
            if (currentPage == null)
            {
                return new EmptyResult();
            }

            var viewModel = CreateCheckoutViewModel(currentPage);
            ModelState.AddModelError("Purchase", filterContext.Exception.Message);

            return View(viewModel.ViewName, viewModel);
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            _controllerExceptionHandler.HandleRequestValidationException(filterContext, "purchase", OnPurchaseException);
        }

        private void UpdateShippingMethodIds(ICart cart, IList<ShipmentViewModel> shipments)
        {
            var i = 0;
            foreach (var shipment in cart.Forms.First().Shipments)
            {
                shipment.ShippingMethodId = shipments[i].ShippingMethodId;
                i++;
            }

            Cart.ApplyDiscounts(_promotionEngine, new PromotionEngineSettings());            
        }

        private void UpdateAddressNames(CheckoutViewModel viewModel)
        {
            Guid guid;
            if (Guid.TryParse(viewModel.BillingAddress.Name, out guid))
            {
                viewModel.BillingAddress.Name = "Billing address (" + viewModel.BillingAddress.Line1 + ")";
            }

            if (!viewModel.UseBillingAddressForShipment)
            {
                foreach (var address in viewModel.Shipments.Select(x => x.Address))
                {
                    if (Guid.TryParse(address.Name, out guid))
                    {
                        address.Name = "Shipping address (" + address.Line1 + ")";
                    }
                }
            }
        }

        private ICart Cart
        {
            get { return _cart ?? (_cart = _cartService.LoadCart(_cartService.DefaultCartName)); }
        }

        private bool CartIsNullOrEmpty()
        {
            return Cart == null || !Cart.GetAllLineItems().Any();
        }

        private CheckoutViewModel ChangeShippingAddress(UpdateAddressViewModel updateViewModel)
        {
            var shippingAddressUpdated = updateViewModel.ShippingAddressIndex > -1;
            var updatedAddress = shippingAddressUpdated ? updateViewModel.Shipments[updateViewModel.ShippingAddressIndex].Address : updateViewModel.BillingAddress;

            if (updatedAddress.AddressId == null)
            {
                updatedAddress = new AddressModel
                {
                    Name = Guid.NewGuid().ToString(),
                };
            }
            else
            {
                _addressBookService.LoadAddress(updatedAddress);
            }

            _addressBookService.LoadCountriesAndRegionsForAddress(updatedAddress);
            var viewModel = CreateCheckoutViewModel(updateViewModel.CurrentPage);

            viewModel.UseBillingAddressForShipment = updateViewModel.UseBillingAddressForShipment;
            viewModel.BillingAddress = updateViewModel.BillingAddress;

            if (shippingAddressUpdated)
            {
                _addressBookService.LoadAddress(viewModel.BillingAddress);
                _addressBookService.LoadCountriesAndRegionsForAddress(viewModel.BillingAddress);
                _addressBookService.LoadAddress(updatedAddress);
                viewModel.Shipments[updateViewModel.ShippingAddressIndex].Address = updatedAddress;
            }
            else
            {
                viewModel.BillingAddress = updatedAddress;
                for (var i = 0; i < viewModel.Shipments.Count; i++)
                {
                    viewModel.Shipments[i].Address = updateViewModel.Shipments[i].Address;
                }
            }

            foreach (var shipment in viewModel.Shipments)
            {
                _addressBookService.LoadCountriesAndRegionsForAddress(shipment.Address);
            }

            return viewModel;
        }
    }
}
