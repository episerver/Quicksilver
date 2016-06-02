using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Models;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Services;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.Exceptions;
using EPiServer.Reference.Commerce.Site.Features.Payment.Models;
using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;
using EPiServer.Reference.Commerce.Site.Features.Payment.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
using Mediachase.BusinessFoundation.Data.Business;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Marketing;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Website;
using Mediachase.Commerce.Website.Helpers;
using Newtonsoft.Json;
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
        private readonly ICartService _cartService;
        private readonly IContentRepository _contentRepository;
        private readonly UrlResolver _urlResolver;
        private readonly IMailService _mailService;
        private readonly ICheckoutService _checkoutService;
        private readonly IPaymentService _paymentService;
        private readonly IContentLoader _contentLoader;
        private readonly LocalizationService _localizationService;
        private readonly Func<string, CartHelper> _cartHelper;
        private readonly ICurrencyService _currencyService;
        private readonly IAddressBookService _addressBookService;
        private const string _billingAddressPrefix = "BillingAddress";
        private const string _shippingAddressPrefix = "ShippingAddresses[{0}]";
        private readonly ControllerExceptionHandler _controllerExceptionHandler;
        private readonly CustomerContextFacade _customerContext;
        private const string CouponKey = "CouponCookieKey";
        private readonly CookieService _cookieService;
        private readonly ILineItemCalculator _LineItemCalculator;

        public CheckoutController(
                    ICartService cartService,
                    IContentRepository contentRepository,
                    UrlResolver urlResolver,
                    IMailService mailService,
                    ICheckoutService checkoutService,
                    IContentLoader contentLoader,
                    IPaymentService paymentService,
                    LocalizationService localizationService,
                    Func<string, CartHelper> cartHelper,
                    ICurrencyService currencyService,
                    IAddressBookService addressBookService,
                    ControllerExceptionHandler controllerExceptionHandler,
                    CustomerContextFacade customerContextFacade,
                    CookieService cookieService,
                    ILineItemCalculator lineItemCalculator)
        {
            _cartService = cartService;
            _contentRepository = contentRepository;
            _urlResolver = urlResolver;
            _mailService = mailService;
            _checkoutService = checkoutService;
            _paymentService = paymentService;
            _contentLoader = contentLoader;
            _localizationService = localizationService;
            _cartHelper = cartHelper;
            _currencyService = currencyService;
            _addressBookService = addressBookService;
            _controllerExceptionHandler = controllerExceptionHandler;
            _customerContext = customerContextFacade;
            _cookieService = cookieService;
            _LineItemCalculator = lineItemCalculator;
        }

        [HttpGet]
        public ActionResult Index(CheckoutPage currentPage)
        {
            var currency = _currencyService.GetCurrentCurrency();
            if (currency != _cartHelper(Mediachase.Commerce.Orders.Cart.DefaultName).Cart.BillingCurrency)
            {
                _cartHelper(Mediachase.Commerce.Orders.Cart.DefaultName).Cart.BillingCurrency = currency;
                _cartService.SaveCart();
            }

            ApplyCoupons();

            var viewModel = CreateCheckoutViewModel(currentPage);
            _cartService.SaveCart();

            return View(viewModel.ViewName, viewModel);
        }

        [HttpGet]
        public ActionResult MultiShipment(CheckoutPage currentPage)
        {
            MultiShipmentViewModel viewModel = new MultiShipmentViewModel();
            InitializeMultiShipmentViewModel(viewModel);

            return View("MultiShipmentAddressSelection", viewModel);
        }

        [HttpPost]
        public ActionResult MultiShipment(CheckoutPage currentPage, MultiShipmentViewModel viewModel)
        {
            for (int i = 0; i < viewModel.CartItems.Count(); i++)
            {
                if (viewModel.CartItems[i].AddressId == null)
                {
                    ModelState.AddModelError(string.Format("CartItems[{0}].AddressId", i), _localizationService.GetString("/Checkout/MultiShipment/Empty/AddressId"));
                }
            }

            if (!ModelState.IsValid)
            {
                InitializeMultiShipmentViewModel(viewModel);
                return View("MultiShipmentAddressSelection", viewModel);
            }

            var currentCurrency = _currencyService.GetCurrentCurrency();

            var checkoutViewModel = HttpContext.User.Identity.IsAuthenticated ?
                GetAuthenticatedCheckoutViewModelForMultiShipment(currentPage, viewModel) :
                GetAnonymousCheckoutViewModelForMultiShipment(currentPage, viewModel);

            checkoutViewModel.UseBillingAddressForShipment = false;

            _checkoutService.CreateShipments(checkoutViewModel.CartItems, checkoutViewModel.ShippingAddresses);

            foreach (var item in checkoutViewModel.CartItems)
            {
                item.ExtendedPrice = _LineItemCalculator.GetExtendedPrice(item, currentCurrency);
            }

            return View(checkoutViewModel.ViewName, checkoutViewModel);
        }

        private CheckoutViewModel GetAnonymousCheckoutViewModelForMultiShipment(CheckoutPage currentPage, MultiShipmentViewModel viewModel)
        {
            var checkoutViewModel = CreateCheckoutViewModel(currentPage);
            var countries = checkoutViewModel.BillingAddress.CountryOptions;
            var selectedShippingMethodId = checkoutViewModel.ShippingMethodViewModels.First().Id;

            checkoutViewModel.CartItems = viewModel.CartItems.AggregateCartItems(_cartService.GetCartItems());

            MergeAnonymousShippingAddresses(viewModel);

            checkoutViewModel.ShippingAddresses = viewModel.AvailableAddresses;

            foreach (var address in checkoutViewModel.ShippingAddresses)
            {
                address.ShippingMethodId = selectedShippingMethodId;
                address.CountryOptions = countries;
            }

            return checkoutViewModel;
        }

        private CheckoutViewModel GetAuthenticatedCheckoutViewModelForMultiShipment(CheckoutPage currentPage, MultiShipmentViewModel viewModel)
        {
            var checkoutViewModel = CreateCheckoutViewModel(currentPage);
            var selectedShippingMethodId = checkoutViewModel.ShippingMethodViewModels.First().Id;

            checkoutViewModel.CartItems = viewModel.CartItems.AggregateCartItems(_cartService.GetCartItems());

            checkoutViewModel.ShippingAddresses = checkoutViewModel.CartItems
              .Select(x => x.AddressId).Distinct()
              .Select(a => (new ShippingAddress { AddressId = a.Value, ShippingMethodId = selectedShippingMethodId }))
              .ToList();

            foreach (var address in checkoutViewModel.ShippingAddresses)
            {
                _addressBookService.LoadAddress(address);
            }

            return checkoutViewModel;
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult Update(CheckoutViewModel viewModel, IPaymentMethodViewModel<IPaymentOption> paymentViewModel)
        {
            // Since the payment property is marked with an exclude binding attribute in the CheckoutViewModel
            // it needs to be manually re-added again.
            viewModel.Payment = paymentViewModel;

            InitializeCheckoutViewModel(viewModel);

            ModelState.Clear();

            return PartialView("Partial", viewModel);
        }

        /// <summary>
        /// Changes an address in the checkout view to one of the existing customer addresses.
        /// </summary>
        /// <param name="viewModel">The view model representing the purchase order.</param>
        /// <param name="shippingAddressIndex">The index of the shipping address to be changed. If it concerns the billing address and not a shipping address then this value will be -1.</param>
        /// <returns>A refreshed view containing the billing address and all the shipping addresses.</returns>
        [HttpPost]
        [AllowDBWrite]
        public ActionResult ChangeAddress(CheckoutViewModel viewModel, int shippingAddressIndex)
        {
            ModelState.Clear();

            bool shippingAddressUpdated = shippingAddressIndex > -1;
            ShippingAddress updatedAddress = (shippingAddressUpdated) ? viewModel.ShippingAddresses[shippingAddressIndex] : viewModel.BillingAddress;

            InitializeCheckoutViewModel(viewModel);

            if (updatedAddress.AddressId == null)
            {
                // If the address id is an empty guid it means that a new empty address should be used. The only thing we
                // keep is the id of currently selected shipping method.
                updatedAddress = new ShippingAddress
                {
                    ShippingMethodId = updatedAddress.ShippingMethodId,
                    Name = _localizationService.GetString("/Shared/Address/NewAddress") + " " + viewModel.AvailableAddresses.Count()
                };
            }

            _addressBookService.LoadAddress(updatedAddress);

            if (shippingAddressUpdated)
            {
                viewModel.ShippingAddresses[shippingAddressIndex] = updatedAddress;
                updatedAddress.HtmlFieldPrefix = string.Format(_shippingAddressPrefix, shippingAddressIndex);
            }
            else
            {
                viewModel.BillingAddress = updatedAddress;
                updatedAddress.HtmlFieldPrefix = _billingAddressPrefix;
            }

            PopulateCountryAndRegions(viewModel, updatedAddress);

            var addressViewName = viewModel.ViewName == "SingleShipmentCheckout" ? "AllAddresses" : "BillingAddress";

            return PartialView(addressViewName, viewModel);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult OrderSummary()
        {
            var orderForm = _cartService.GetOrderForms();

            OrderSummaryViewModel viewModel = new OrderSummaryViewModel
            {
                SubTotal = _cartService.GetSubTotal(),
                CartTotal = _cartService.GetTotal(),
                ShippingTotal = _cartService.GetShippingTotal(),
                ShippingSubtotal = _cartService.GetShippingSubTotal(),
                OrderDiscountTotal = _cartService.GetOrderDiscountTotal(),
                ShippingDiscountTotal = _cartService.GetShippingDiscountTotal(),
                ShippingTaxTotal = _cartService.GetShippingTaxTotal(),
                TaxTotal = _cartService.GetTaxTotal(),
                OrderDiscounts = orderForm.SelectMany(x => x.Discounts.Cast<OrderFormDiscount>()).Select(x => new OrderDiscountModel
                {
                    Discount = _cartService.ConvertToMoney(x.DiscountValue),
                    Displayname = x.DisplayMessage
                }),
                ShippingDiscounts = orderForm.SelectMany(x => x.Shipments).SelectMany(x => x.Discounts.Cast<ShipmentDiscount>()).Select(x => new OrderDiscountModel
                {
                    Discount = _cartService.ConvertToMoney(x.DiscountValue),
                    Displayname = x.DisplayMessage
                })
            };

            return PartialView(viewModel);
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult AddCouponCode(CheckoutPage currentPage, string couponCode)
        {
            MarketingContext.Current.AddCouponToMarketingContext(couponCode);
            _cartService.RunWorkflow(OrderGroupWorkflowManager.CartValidateWorkflowName);
            _cartService.SaveCart();

            if (!GetAppliedDiscountsWithCode().Where(d => couponCode.Equals(d.DiscountCode)).Any())
            {
                return new EmptyResult();
            }

            var cookie = _cookieService.Get(CouponKey);
            var coupons = cookie != null ? JsonConvert.DeserializeObject<HashSet<string>>(cookie) : new HashSet<string>();
            coupons.Add(couponCode);
            _cookieService.Set(CouponKey, JsonConvert.SerializeObject(coupons));

            var viewModel = CreateCheckoutViewModel(currentPage);

            return View(viewModel.ViewName, viewModel);
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult RemoveCouponCode(CheckoutPage currentPage, string couponCode)
        {
            var removeDiscounts = GetAppliedDiscountsWithCode().Where(d => couponCode.Equals(d.DiscountCode));
            foreach (var discount in removeDiscounts)
            {
                discount.Delete();
            }

            var cookie = _cookieService.Get(CouponKey);
            if (cookie != null)
            {
                var coupons = JsonConvert.DeserializeObject<HashSet<string>>(cookie);
                if (coupons != null && coupons.Contains(couponCode))
                {
                    coupons.Remove(couponCode);
                    _cookieService.Set(CouponKey, JsonConvert.SerializeObject(coupons));
                }
            }

            var viewModel = CreateCheckoutViewModel(currentPage);

            return View(viewModel.ViewName, viewModel);
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult Purchase(CheckoutViewModel checkoutViewModel, IPaymentMethodViewModel<IPaymentOption> paymentViewModel)
        {
            // Since the payment property is marked with an exclude binding attribute in the CheckoutViewModel
            // it needs to be manually re-added again.
            checkoutViewModel.Payment = paymentViewModel;

            ApplyCoupons();

            if (String.IsNullOrEmpty(checkoutViewModel.BillingAddress.Email))
            {
                ModelState.AddModelError("BillingAddress.Email", _localizationService.GetString("/Shared/Address/Form/Empty/Email"));
            }

            if (checkoutViewModel.UseBillingAddressForShipment)
            {
                // If only the billing address is of interest we need to remove any existing error related to the
                // other shipping addresses.
                foreach (var state in ModelState.Where(x => x.Key.StartsWith("ShippingAddresses")).ToArray())
                {
                    ModelState.Remove(state);
                }
            }

            if (!ModelState.IsValid)
            {
                InitializeCheckoutViewModel(checkoutViewModel);
                return View(checkoutViewModel.ViewName, checkoutViewModel);
            }

            _checkoutService.ClearOrderAddresses();

            // If the shipping address should be the same as the billing address, replace all existing shipping addresses to
            // be the same as the billing address instead.
            if (checkoutViewModel.UseBillingAddressForShipment)
            {
                checkoutViewModel.ShippingAddresses = new ShippingAddress[] { checkoutViewModel.BillingAddress };
            }

            SaveBillingAddress(checkoutViewModel);

            if (!SaveShippingAddresses(checkoutViewModel))
            {
                InitializeCheckoutViewModel(checkoutViewModel);
                return View(checkoutViewModel.ViewName, checkoutViewModel);
            }

            _cartService.RunWorkflow(OrderGroupWorkflowManager.CartPrepareWorkflowName);
            _cartService.SaveCart();

            try
            {
                _paymentService.ProcessPayment(checkoutViewModel.Payment.PaymentMethod);
            }
            catch (PreProcessException)
            {
                ModelState.AddModelError("PaymentMethod", _localizationService.GetString("/Checkout/Payment/Errors/PreProcessingFailure"));
            }

            if (!ModelState.IsValid)
            {
                InitializeCheckoutViewModel(checkoutViewModel);
                return View(checkoutViewModel.ViewName, checkoutViewModel);
            }

            return Finish(checkoutViewModel.CurrentPage);
        }

        /// <summary>
        /// Finalizes a purchase orders and send an e-mail confirmation to the customer.
        /// </summary>
        /// <param name="currentPage">The checkout content page.</param>
        /// <returns>The confirmation view for the purchase order.</returns>
        [HttpGet]
        public ActionResult Finish(CheckoutPage currentPage)
        {
            if (!_cartService.GetCartItems().Any())
            {
                return RedirectToAction("Index");
            }

            var startpage = _contentRepository.Get<StartPage>(ContentReference.StartPage);
            var confirmationPage = _contentRepository.GetFirstChild<OrderConfirmationPage>(currentPage.ContentLink);
            PurchaseOrder purchaseOrder = null;
            string emailAddress = null;
            OrderForm orderForm = _cartService.GetOrderForms().First();
            decimal totalProcessedAmount = orderForm.Payments.Where(x => x.Status.Equals(PaymentStatus.Processed.ToString())).Sum(x => x.Amount);

            if (totalProcessedAmount != orderForm.Total)
            {
                throw new InvalidOperationException("Wrong amount");
            }

            _cartService.RunWorkflow(OrderGroupWorkflowManager.CartCheckOutWorkflowName);
            purchaseOrder = _checkoutService.SaveCartAsPurchaseOrder();

            _checkoutService.DeleteCart();

            emailAddress = purchaseOrder.OrderAddresses.First().Email;

            var queryCollection = new NameValueCollection
            {
                {"contactId", _customerContext.CurrentContactId.ToString()},
                {"orderNumber", purchaseOrder.OrderGroupId.ToString(CultureInfo.InvariantCulture)}
            };

            try
            {
                _mailService.Send(startpage.OrderConfirmationMail, queryCollection, emailAddress, currentPage.Language.Name);
            }
            catch (Exception)
            {
                // The purchase has been processed and the payment was successfully settled, but for some reason the e-mail
                // receipt could not be sent to the customer. Rollback is not possible so simple make sure to inform the
                // customer to print the confirmation page instead.
                queryCollection.Add("notificationMessage", string.Format(_localizationService.GetString("/OrderConfirmationMail/ErrorMessages/SmtpFailure"), emailAddress));

                // Todo: Log the error and raise an alert so that an administrator can look in to it.
            }

            _cookieService.Remove(CouponKey);

            return Redirect(new UrlBuilder(confirmationPage.LinkURL) { QueryCollection = queryCollection }.ToString());
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

        /// <summary>
        /// Creates and returns a new instance if a CheckoutViewModel.
        /// </summary>
        /// <param name="currentPage">The checkout content page.</param>
        /// <returns>A new instance of a <see cref="CheckoutViewModel"/>.</returns>
        private CheckoutViewModel CreateCheckoutViewModel(CheckoutPage currentPage)
        {
            var viewModel = new CheckoutViewModel();
            var customer = _customerContext.CurrentContact.CurrentContact;

            var billingAddress = new ShippingAddress
            {
                AddressId = customer != null ? customer.PreferredBillingAddressId : null,
                Name = _localizationService.GetString("/Shared/Address/NewAddress"),
                HtmlFieldPrefix = _billingAddressPrefix
            };

            // If the customer uses the same address for billing and shipment then we prepare a new empty address as shipping address
            // in case the customer chooses to not use the billing address after all.
            var shippingAddress = new ShippingAddress
            {
                AddressId = (customer != null && customer.PreferredShippingAddressId != customer.PreferredBillingAddressId) ? customer.PreferredShippingAddressId : null,
                Name = _localizationService.GetString("/Shared/Address/NewAddress"),
                HtmlFieldPrefix = _shippingAddressPrefix
            };

            viewModel = new CheckoutViewModel
            {
                BillingAddress = billingAddress,
                UseBillingAddressForShipment = (customer == null || (customer.PreferredShippingAddressId.HasValue && customer.PreferredShippingAddressId == customer.PreferredBillingAddressId)),
                ShippingAddresses = new ShippingAddress[] { shippingAddress },
                CartItems = _cartService.GetCartItems(),
                CurrentPage = currentPage
            };

            InitializeCheckoutViewModel(viewModel);

            return viewModel;
        }

        private void SetPaymentMethods(CheckoutViewModel viewModel)
        {
            var paymentMethods = _checkoutService.GetPaymentMethods();

            viewModel.PaymentMethodViewModels = paymentMethods;

            if (viewModel.Payment == null)
            {
                PaymentMethodViewModel<IPaymentOption> selectedPaymentMethod = paymentMethods.First();

                viewModel.Payment = PaymentMethodViewModelResolver.Resolve(selectedPaymentMethod.SystemName);
                viewModel.Payment.Description = selectedPaymentMethod.Description;
                viewModel.Payment.SystemName = selectedPaymentMethod.SystemName;
                ((PaymentMethodBase)viewModel.Payment.PaymentMethod).PaymentMethodId = selectedPaymentMethod.Id;
            }
        }

        private IList<ShippingAddress> GetAvailableShippingAddresses()
        {
            var addresses = _addressBookService.GetAvailableShippingAddresses();
            foreach (var address in addresses.Where(x => string.IsNullOrEmpty(x.Name)))
            {
                address.Name = _localizationService.GetString("/Shared/Address/DefaultAddressName");
            }

            return addresses;
        }

        private void InitializeMultiShipmentViewModel(MultiShipmentViewModel viewModel)
        {
            var cartItems = _cartService.GetCartItems();
            var currency = _currencyService.GetCurrentCurrency();

            if (viewModel.CartItems == null)
            {
                viewModel.CartItems = cartItems.ToFlattenedArray();
            }

            viewModel.StartPage = _contentRepository.Get<StartPage>(ContentReference.StartPage);
            viewModel.AvailableAddresses = GetAvailableShippingAddresses();
            viewModel.ReferrerUrl = GetReferrerUrl();

            foreach (var item in viewModel.CartItems)
            {
                item.Currency = currency;
                item.SetValues(cartItems.Single(x => x.Code == item.Code));
            }

            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                foreach (var item in viewModel.CartItems)
                {
                    var anonymousShippingAddress = new ShippingAddress
                    {
                        AddressId = Guid.NewGuid(),
                        Name = "Anonymous",
                        HtmlFieldPrefix = _shippingAddressPrefix,
                        CountryCode = "SWE"
                    };

                    item.AddressId = anonymousShippingAddress.AddressId;
                    _addressBookService.GetCountriesAndRegionsForAddress(anonymousShippingAddress);
                    viewModel.AvailableAddresses.Add(anonymousShippingAddress);
                }
            }
        }

        /// <summary>
        /// Inspects all available shipping addresses in a <see cref="MultiShipmentViewModel"/> and
        /// cleans up and merges address having similar values.
        /// </summary>
        /// <param name="viewModel">A <see cref="MultiShipmentViewModel"/> having the shipping addresses to be merged.</param>
        private void MergeAnonymousShippingAddresses(MultiShipmentViewModel viewModel)
        {
            for (int i = viewModel.AvailableAddresses.Count - 1; i >= 0; i--)
            {
                var currentAddress = viewModel.AvailableAddresses[i];
                var addressHasBeenReplaced = false;

                foreach (var address in viewModel.AvailableAddresses.Where(x => x != currentAddress))
                {
                    if (address.FirstName == currentAddress.FirstName &&
                        address.LastName == currentAddress.LastName &&
                        address.Line1 == currentAddress.Line1 &&
                        address.Line2 == currentAddress.Line2 &&
                        address.Organization == currentAddress.Organization &&
                        address.PostalCode == currentAddress.PostalCode &&
                        address.City == currentAddress.City &&
                        address.CountryCode == currentAddress.CountryCode)
                    {
                        foreach (var item in viewModel.CartItems.Where(x => x.AddressId == currentAddress.AddressId))
                        {
                            item.AddressId = address.AddressId;
                        }

                        addressHasBeenReplaced = true;
                    }
                }

                if (addressHasBeenReplaced)
                {
                    viewModel.AvailableAddresses.Remove(currentAddress);
                }
            }
        }

        /// <summary>
        /// Initializes an existing <see cref="CheckoutViewModel"/> and loads all its reference data.
        /// </summary>
        /// <param name="viewModel">A <see cref="CheckoutViewModel"/> that should be populated with addresses, shipping- and payment details.</param>
        private void InitializeCheckoutViewModel(CheckoutViewModel viewModel)
        {
            ManageCartItems(viewModel);

            SetViewModelCurrency(viewModel);
            SetPaymentMethods(viewModel);

            var shippingAddresses = (viewModel.UseBillingAddressForShipment) ? new ShippingAddress[] { viewModel.BillingAddress } : viewModel.ShippingAddresses;
            var shipment = _checkoutService.CreateShipments(viewModel.CartItems, shippingAddresses).FirstOrDefault();

            if (shipment != null)
            {
                var shippingRates = _checkoutService.GetShippingRates(shipment);
                var shippingMethods = GetShippingMethods(shippingRates);
                var defaultShippingMethodId = shippingMethods.First().Id;

                viewModel.ShippingMethodViewModels = shippingMethods;

                foreach (var address in viewModel.ShippingAddresses)
                {
                    _addressBookService.LoadAddress(address);

                    if (address.ShippingMethodId == Guid.Empty)
                    {
                        address.ShippingMethodId = defaultShippingMethodId;
                    }
                }
            }

            _addressBookService.LoadAddress(viewModel.BillingAddress);

            // Countries and region lists are always lost during postbacks.
            // Therefor get all countries and regions for the billing address.
            _addressBookService.GetCountriesAndRegionsForAddress(viewModel.BillingAddress);

            // And get the countries and the regions for all other addresses as well.
            PopulateCountryAndRegions(viewModel, viewModel.BillingAddress);

            viewModel.StartPage = _contentLoader.Get<StartPage>(ContentReference.StartPage);
            viewModel.ReferrerUrl = GetReferrerUrl();
            viewModel.AppliedCouponCodes = GetAppliedDiscountsWithCode().Select(d => d.DiscountCode).Distinct();
            viewModel.AvailableAddresses = GetAvailableShippingAddresses();

            // Add an extra dummy address for creating new addresses.
            viewModel.AvailableAddresses.Add(new ShippingAddress
            {
                Name = _localizationService.GetString("/Shared/Address/NewAddress")
            });
        }

        private void ManageCartItems(CheckoutViewModel viewModel)
        {
            var cartItems = _cartService.GetCartItems();
            if (viewModel.CartItems != null)
            {
                foreach (var item in viewModel.CartItems)
                {
                    var product = cartItems.Single(x => x.Code == item.Code);
                    item.SetValues(product);
                    item.ExtendedPrice = _LineItemCalculator.GetExtendedPrice(item, item.Currency);
                }
            }
            else
            {
                viewModel.CartItems = cartItems;
            }
        }

        private void SetViewModelCurrency(CheckoutViewModel viewModel)
        {
            var currency = _currencyService.GetCurrentCurrency();
            foreach (var item in viewModel.CartItems)
            {
                item.Currency = currency;
            }
        }

        private string GetReferrerUrl()
        {
            if (HttpContext.Request.Url != null)
            {
                if (HttpContext.Request.UrlReferrer != null &&
                    HttpContext.Request.UrlReferrer.Host.Equals(HttpContext.Request.Url.Host, StringComparison.OrdinalIgnoreCase))
                {
                    return HttpContext.Request.UrlReferrer.ToString();
                }
            }
            return _urlResolver.GetUrl(ContentReference.StartPage);
        }

        private IEnumerable<ShippingMethodViewModel> GetShippingMethods(IEnumerable<ShippingRate> shippingRates)
        {
            return shippingRates.Select(r => new ShippingMethodViewModel
            {
                Id = r.Id,
                DisplayName = r.Name,
                Price = r.Money,
            });
        }

        /// <summary>
        /// Adds countries and regions to all addresses in a CheckoutViewModel.
        /// </summary>
        /// <param name="viewModel">The view model containing the addresses that needs the countries and regions.</param>
        /// <param name="updatedAddress">The most recently updated address. This one should already hold a reference to all existing countries.</param>
        /// <remarks>
        /// Each country in the view model may´be a subject of being presented as a dropdown in the view. It's therefor important to make sure it
        /// holds a reference to all countries along with any applicable regions. We can re-use the countries that has already been fetched for the
        /// most recently updated address. It may also have all the regions we need. This method goes through all addresses and applies all the countries
        /// and related regions. If the country of an address is different from the updated address then we only need to get a fresh set of regions
        /// for that perticular address.
        /// </remarks>
        private void PopulateCountryAndRegions(CheckoutViewModel viewModel, ShippingAddress updatedAddress)
        {
            List<ShippingAddress> addressCollection = new List<ShippingAddress>();
            if (viewModel.ShippingAddresses != null)
            {
                addressCollection.AddRange(viewModel.ShippingAddresses);
            }
            addressCollection.Add(viewModel.BillingAddress);

            // Go through all address that we know haven't been updated by checking if country collection still is null.
            foreach (ShippingAddress unchangedAddress in addressCollection.Where(x => x.CountryOptions == null))
            {
                unchangedAddress.CountryOptions = updatedAddress.CountryOptions;

                // By comparing countries, see if we can re-use the already existing regions or if we must fetch new ones.
                if (unchangedAddress.CountryCode == updatedAddress.CountryCode)
                {
                    unchangedAddress.RegionOptions = updatedAddress.RegionOptions;
                }
                else
                {
                    unchangedAddress.RegionOptions = _addressBookService.GetRegionOptionsByCountryCode(unchangedAddress.CountryCode);
                }
            }
        }

        private IEnumerable<Discount> GetDiscountsWithCode(Func<OrderForm, IEnumerable<Discount>> getDiscountsFunc)
        {
            return _cartService.GetOrderForms().SelectMany(form => getDiscountsFunc(form)).Where(d => !string.IsNullOrEmpty(d.DiscountCode));
        }

        /// <summary>
        /// Gets a collection of applied discounts that have coupon code.
        /// It includes order level discount, entry level discount and shipment level discount.
        /// </summary>
        /// <returns>A collection of applied discounts.</returns>
        private IEnumerable<Discount> GetAppliedDiscountsWithCode()
        {
            var appliedDiscounts = new List<Discount>();

            // Get order level discounts
            appliedDiscounts.AddRange(GetDiscountsWithCode(form => form.Discounts.Cast<Discount>()));
            // Get entry level discounts
            appliedDiscounts.AddRange(GetDiscountsWithCode(form => form.LineItems.SelectMany(l => l.Discounts.Cast<Discount>())));
            // Get shipment level discounts
            appliedDiscounts.AddRange(GetDiscountsWithCode(form => form.Shipments.SelectMany(s => s.Discounts.Cast<Discount>())));

            return appliedDiscounts;
        }

        /// <summary>
        /// Converts the billing address into an order address and saves it. If any changes to the address should be saved for future usage
        /// then the address will also be converted as a customer addresses and gets related to the current contact.
        /// </summary>
        /// <param name="checkoutViewModel">The view model representing the purchase order.</param>
        private void SaveBillingAddress(CheckoutViewModel checkoutViewModel)
        {
            var orderAddress = _checkoutService.AddNewOrderAddress();

            _addressBookService.MapModelToOrderAddress(checkoutViewModel.BillingAddress, orderAddress);
            orderAddress.Name = Guid.NewGuid().ToString();
            orderAddress.AcceptChanges();
            _checkoutService.UpdateBillingAddressId(orderAddress.Name);

            SaveToAddressBookIfNeccessary(checkoutViewModel.BillingAddress);
        }

        /// <summary>
        /// Converts and saves all shipping addresses for a purchase into order addresses. If one or more addresses should be saved for future usage
        /// then they are also stored as customer addresses and gets related to the current contact.
        /// </summary>
        /// <param name="checkoutViewModel">The view model representing the purchase order.</param>
        /// <returns><c>true</c> if there save was successful, otherwise <c>false</c>.</returns>
        private bool SaveShippingAddresses(CheckoutViewModel checkoutViewModel)
        {
            if (checkoutViewModel.ShippingAddresses == null ||
                !checkoutViewModel.ShippingAddresses.Any(address => address.ShippingMethodId != Guid.Empty))
            {
                return false;
            }

            var shipments = _cartService.GetShipments();

            for (int index = 0; index < shipments.Count(); index++)
            {
                var orderAddress = _checkoutService.AddNewOrderAddress();
                var shippingAddress = checkoutViewModel.ShippingAddresses[index];
                var shipment = shipments.ElementAt(index);

                _addressBookService.MapModelToOrderAddress(shippingAddress, orderAddress);
                orderAddress.Name = Guid.NewGuid().ToString();
                orderAddress.AcceptChanges();

                shipment.ShippingAddressId = orderAddress.Name;
                var shippingRate = _checkoutService.GetShippingRate(shipment, shippingAddress.ShippingMethodId);
                _checkoutService.UpdateShipment(shipment, shippingRate);

                SaveToAddressBookIfNeccessary(shippingAddress);
            }

            return true;
        }

        private void SaveToAddressBookIfNeccessary(ShippingAddress address)
        {
            if (address.SaveAddress && User.Identity.IsAuthenticated && _addressBookService.CanSave(address))
            {
                var currentContact = _customerContext.CurrentContact.CurrentContact;
                var customerAddress = currentContact.ContactAddresses.FirstOrDefault(x => x.AddressId == address.AddressId) ?? CustomerAddress.CreateInstance();

                _addressBookService.MapModelToCustomerAddress(address, customerAddress);

                if (address.AddressId == null)
                {
                    currentContact.AddContactAddress(customerAddress);
                }
                else
                {
                    BusinessManager.Update(customerAddress);
                }
                currentContact.SaveChanges();
            }
        }

        private void ApplyCoupons()
        {
            var cookie = _cookieService.Get(CouponKey);
            if (cookie != null)
            {
                var coupons = JsonConvert.DeserializeObject<HashSet<string>>(cookie);
                if (coupons != null && coupons.Any())
                {
                    foreach (var coupon in coupons)
                    {
                        MarketingContext.Current.AddCouponToMarketingContext(coupon);
                    }
                }
            }
        }
    }
}
