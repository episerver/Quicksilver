using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Recommendations.Commerce.Tracking;
using EPiServer.Recommendations.Tracking;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;
using EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using EPiServer.Web.Mvc;
using EPiServer.Web.Mvc.Html;
using EPiServer.Web.Routing;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Services;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Controllers
{
    public class CheckoutController : PageController<CheckoutPage>
    {
        private readonly ICurrencyService _currencyService;
        private readonly ControllerExceptionHandler _controllerExceptionHandler;
        private readonly CheckoutViewModelFactory _checkoutViewModelFactory;
        private readonly OrderSummaryViewModelFactory _orderSummaryViewModelFactory;
        private readonly IOrderRepository _orderRepository;
        private readonly ICartService _cartService;
        private readonly IRecommendationService _recommendationService;
        private ICart _cart;
        private readonly CheckoutService _checkoutService;

        public CheckoutController(
            ICurrencyService currencyService,
            ControllerExceptionHandler controllerExceptionHandler,
            IOrderRepository orderRepository,
            CheckoutViewModelFactory checkoutViewModelFactory,
            ICartService cartService,
            OrderSummaryViewModelFactory orderSummaryViewModelFactory,
            IRecommendationService recommendationService,
            CheckoutService checkoutService)
        {
            _currencyService = currencyService;
            _controllerExceptionHandler = controllerExceptionHandler;
            _orderRepository = orderRepository;
            _checkoutViewModelFactory = checkoutViewModelFactory;
            _cartService = cartService;
            _orderSummaryViewModelFactory = orderSummaryViewModelFactory;
            _recommendationService = recommendationService;
            _checkoutService = checkoutService;
        }

        [HttpGet]
        [OutputCache(Duration = 0, NoStore = true)]
        [Tracking(TrackingType.Checkout)]
        public ActionResult Index(CheckoutPage currentPage)
        {
            if (CartIsNullOrEmpty())
            {
                return View("EmptyCart");
            }

            var viewModel = CreateCheckoutViewModel(currentPage);

            Cart.Currency = _currencyService.GetCurrentCurrency();
            
            if (User.Identity.IsAuthenticated)
            {
                _checkoutService.UpdateShippingAddresses(Cart, viewModel);
            }

            _checkoutService.UpdateShippingMethods(Cart, viewModel.Shipments);
            _checkoutService.ApplyDiscounts(Cart);
            _orderRepository.Save(Cart);

            return View(viewModel.ViewName, viewModel);
        }

        [HttpGet]
        public ActionResult SingleShipment(CheckoutPage currentPage)
        {
            if (!CartIsNullOrEmpty())
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

            _checkoutService.UpdateShippingMethods(Cart, shipmentViewModel.Shipments);
            _checkoutService.ApplyDiscounts(Cart);
            _orderRepository.Save(Cart);

            var viewModel = CreateCheckoutViewModel(currentPage, paymentViewModel);

            return PartialView("Partial", viewModel);
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult ChangeAddress(UpdateAddressViewModel addressViewModel)
        {
            ModelState.Clear();
            var viewModel = CreateCheckoutViewModel(addressViewModel.CurrentPage);
            _checkoutService.CheckoutAddressHandling.ChangeAddress(viewModel, addressViewModel);

            if (User.Identity.IsAuthenticated)
            {
                _checkoutService.UpdateShippingAddresses(Cart, viewModel);
            }
            
            _orderRepository.Save(Cart);

            var addressViewName = addressViewModel.ShippingAddressIndex > -1 ? "SingleShippingAddress" : "BillingAddress";

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
        public ActionResult Purchase(CheckoutViewModel viewModel, IPaymentMethodViewModel<PaymentMethodBase> paymentViewModel)
        {
            if (CartIsNullOrEmpty())
            {
                return Redirect(Url.ContentUrl(ContentReference.StartPage));
            }

            // Since the payment property is marked with an exclude binding attribute in the CheckoutViewModel
            // it needs to be manually re-added again.
            viewModel.Payment = paymentViewModel;
            
            if (User.Identity.IsAuthenticated)
            {
                _checkoutService.CheckoutAddressHandling.UpdateAuthenticatedUserAddresses(viewModel);

                var validation = _checkoutService.AuthenticatedPurchaseValidation;

                if (!validation.ValidateModel(ModelState, viewModel) ||
                    !validation.ValidateOrderOperation(ModelState, _cartService.ValidateCart(Cart)) ||
                    !validation.ValidateOrderOperation(ModelState, _cartService.RequestInventory(Cart)))
                {
                    return View(viewModel);
                }
            }
            else
            {
                _checkoutService.CheckoutAddressHandling.UpdateAnonymousUserAddresses(viewModel);

                var validation = _checkoutService.AnonymousPurchaseValidation;
              
                if (!validation.ValidateModel(ModelState, viewModel) ||
                    !validation.ValidateOrderOperation(ModelState, _cartService.ValidateCart(Cart)) ||
                    !validation.ValidateOrderOperation(ModelState, _cartService.RequestInventory(Cart)))
                {
                    return View(viewModel);
                }
            }

            _checkoutService.UpdateShippingAddresses(Cart, viewModel);
            _checkoutService.CreateAndAddPaymentToCart(Cart, viewModel);

            var purchaseOrder = _checkoutService.PlaceOrder(Cart, ModelState, viewModel);
            if (purchaseOrder == null)
            {
                return View(viewModel);
            }
            
            var confirmationSentSuccessfully = _checkoutService.SendConfirmation(viewModel, purchaseOrder);
          
            _recommendationService.SendOrderTracking(HttpContext, purchaseOrder);

            return Redirect(_checkoutService.BuildRedirectionUrl(viewModel, purchaseOrder, confirmationSentSuccessfully));
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

        private ViewResult View(CheckoutViewModel checkoutViewModel)
        {
            return View(checkoutViewModel.ViewName, CreateCheckoutViewModel(checkoutViewModel.CurrentPage, checkoutViewModel.Payment));
        }

        private CheckoutViewModel CreateCheckoutViewModel(CheckoutPage currentPage, IPaymentMethodViewModel<PaymentMethodBase> paymentViewModel = null)
        {
            return _checkoutViewModelFactory.CreateCheckoutViewModel(Cart, currentPage, paymentViewModel);
        }

        private ICart Cart
        {
            get { return _cart ?? (_cart = _cartService.LoadCart(_cartService.DefaultCartName)); }
        }

        private bool CartIsNullOrEmpty()
        {
            return Cart == null || !Cart.GetAllLineItems().Any();
        }
    }
}
