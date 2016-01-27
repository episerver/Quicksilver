using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
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
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
using Mediachase.BusinessFoundation.Data.Business;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Website;
using Mediachase.Commerce.Website.Helpers;
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
        private readonly CurrencyService _currencyService;
        private readonly IAddressBookService _addressBookService;
        private const string _billingAddressPrefix = "BillingAddress";
        private const string _shippingAddressPrefix = "ShippingAddresses[{0}]";
        private readonly ControllerExceptionHandler _controllerExceptionHandler;
        private readonly CustomerContextFacade _customerContext;

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
                    CurrencyService currencyService,
                    IAddressBookService addressBookService,
                    ControllerExceptionHandler controllerExceptionHandler,
                    CustomerContextFacade customerContextFacade)
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

            CheckoutViewModel viewModel = InitializeCheckoutViewModel(currentPage, null);

            return View(viewModel);
        }

        /// <summary>
        /// Creates and returns a new instance if a CheckoutViewModel.
        /// </summary>
        /// <param name="selectedPaymentMethod">The default payment method to be used for new purchases.</param>
        /// <param name="shippingMethodId">The default shipping method id.</param>
        /// <param name="customer">The currently logged on user. Will be null for anonymous quests.</param>
        /// <returns>A new CheckoutViewModel.</returns>
        private CheckoutViewModel CreateCheckoutViewModel(PaymentMethodViewModel<IPaymentOption> selectedPaymentMethod,
                                                          Guid shippingMethodId,
                                                          CustomerContact customer)
        {
            CheckoutViewModel viewModel = new CheckoutViewModel();

            ShippingAddress billingAddress = new ShippingAddress
            {
                AddressId = customer != null ? customer.PreferredBillingAddressId : null,
                Name = _localizationService.GetString("/Shared/Address/NewAddress"),
                ShippingMethodId = shippingMethodId,
                HtmlFieldPrefix = _billingAddressPrefix
            };

            // If the customer uses the same address for billing and shipment then we prepare a new empty address as shipping address
            // in case the customer chooses to not use the billing address after all.
            ShippingAddress shippingAddress = new ShippingAddress
            {
                AddressId = (customer != null && customer.PreferredShippingAddressId != customer.PreferredBillingAddressId) ? customer.PreferredShippingAddressId : null,
                Name = _localizationService.GetString("/Shared/Address/NewAddress"),
                ShippingMethodId = billingAddress.ShippingMethodId,
                HtmlFieldPrefix = _shippingAddressPrefix
            };

            _addressBookService.LoadAddress(billingAddress);
            _addressBookService.LoadAddress(shippingAddress);

            viewModel = new CheckoutViewModel
            {
                BillingAddress = billingAddress,
                Payment = PaymentMethodViewModelResolver.Resolve(selectedPaymentMethod.SystemName),
                UseBillingAddressForShipment = (customer == null || (customer != null && customer.PreferredShippingAddressId == customer.PreferredBillingAddressId)),
                ShippingAddresses = new ShippingAddress[] { shippingAddress }
            };

            viewModel.Payment.Description = selectedPaymentMethod.Description;
            viewModel.Payment.SystemName = selectedPaymentMethod.SystemName;
            ((PaymentMethodBase)viewModel.Payment.PaymentMethod).PaymentMethodId = selectedPaymentMethod.Id;

            return viewModel;
        }

        private CheckoutViewModel InitializeCheckoutViewModel(CheckoutPage currentPage, CheckoutViewModel viewModel)
        {
            var shipment = _checkoutService.CreateShipment();
            var shippingRates = _checkoutService.GetShippingRates(shipment);
            var shippingMethods = GetShippingMethods(shippingRates);
            var selectedShippingRate = shippingRates.FirstOrDefault();
            var customer = _customerContext.CurrentContact.CurrentContact;
            var paymentMethods = _checkoutService.GetPaymentMethods();

            if (selectedShippingRate != null)
            {
                _checkoutService.UpdateShipment(shipment, selectedShippingRate);
            }
            else
            {
                ModelState.AddModelError("ShippingRate", _localizationService.GetString("/Checkout/Payment/Errors/NoShippingRate"));
            }
            
            if (viewModel == null)
            {
                var shippingMethod = shippingMethods.FirstOrDefault();
                viewModel = CreateCheckoutViewModel(paymentMethods.First(), shippingMethod == null ? Guid.Empty : shippingMethod.Id, customer);

                // Run the workflow once to calculate all taxes, charges and get the correct total amounts.
                _cartService.RunWorkflow(OrderGroupWorkflowManager.CartValidateWorkflowName);
            }
            else
            {
                // Countries and region lists are always lost during postbacks.
                // Therefor get all countries and regions for the billing address.
                _addressBookService.GetCountriesAndRegionsForAddress(viewModel.BillingAddress);

                // And get the countries and the regions for all other addresses as well.
                PopulateCountryAndRegions(viewModel, viewModel.BillingAddress);
            }

            viewModel.StartPage = _contentLoader.Get<StartPage>(ContentReference.StartPage);
            viewModel.ReferrerUrl = GetReferrerUrl();
            viewModel.CurrentPage = currentPage;
            viewModel.PaymentMethodViewModels = paymentMethods;
            viewModel.ShippingMethodViewModels = shippingMethods;
            viewModel.AvailableAddresses = GetAvailableAddresses();

            return viewModel;
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

        private IList<ShippingAddress> GetAvailableAddresses()
        {
            var currentContact = _customerContext.CurrentContact.CurrentContact;
            List<ShippingAddress> addresses = new List<ShippingAddress>();

            if (currentContact != null)
            {
                addresses = currentContact.ContactAddresses.Select(x => new ShippingAddress()
                {
                    AddressId = x.AddressId,
                    Name = !string.IsNullOrEmpty(x.Name) ? x.Name : _localizationService.GetString("/Shared/Address/DefaultAddressName")
                }).ToList();
            }

            //add additional address, representing a new address
            addresses.Add(new ShippingAddress
            {
                Name = _localizationService.GetString("/Shared/Address/NewAddress")
            });

            return addresses;
        }

        [HttpPost]
        public ActionResult Update(CheckoutPage currentPage, CheckoutViewModel viewModel, IPaymentMethodViewModel<IPaymentOption> paymentViewModel)
        {
            // Since the payment property is marked with an exclude binding attribute in the CheckoutViewModel
            // it needs to be manually re-added again.
            viewModel.Payment = paymentViewModel;

            InitializeCheckoutViewModel(currentPage, viewModel);

            IEnumerable<ShippingAddress> shippingAddresses = (viewModel.UseBillingAddressForShipment) ? new ShippingAddress[] { viewModel.BillingAddress } : viewModel.ShippingAddresses;

            foreach (var shippingAddress in shippingAddresses)
            {
                var shipment = _checkoutService.CreateShipment();
                var shippingMethods = _checkoutService.GetShippingRates(shipment);
                var selectedShippingMethod = shippingMethods.FirstOrDefault(x => x.Id == shippingAddress.ShippingMethodId) ?? shippingMethods.First();

                _checkoutService.UpdateShipment(shipment, selectedShippingMethod);
            }

            _cartService.RunWorkflow(OrderGroupWorkflowManager.CartValidateWorkflowName);
            _cartService.SaveCart();

            // Clearing the ModelState will remove any unwanted input validation errors in the new view.
            ModelState.Clear();

            return PartialView("Partial", viewModel);
        }

        /// <summary>
        /// Changes an address in the checkout view to one of the existing customer addresses.
        /// </summary>
        /// <param name="currentPage">The checkout content page.</param>
        /// <param name="viewModel">The view model representing the purchase order.</param>
        /// <param name="shippingAddressIndex">The index of the shipping address to be changed. If it concerns the billing address and not a shipping address then this value will be -1.</param>
        /// <returns>A refreshed view containing the billing address and all the shipping addresses.</returns>
        [HttpPost]
        public ActionResult ChangeAddress(CheckoutPage currentPage, CheckoutViewModel viewModel, int shippingAddressIndex)
        {
            ModelState.Clear();

            bool shippingAddressUpdated = shippingAddressIndex > -1;
            ShippingAddress updatedAddress = (shippingAddressUpdated) ? viewModel.ShippingAddresses[shippingAddressIndex] : viewModel.BillingAddress;

            InitializeCheckoutViewModel(currentPage, viewModel);

            if (updatedAddress.AddressId == null)
            {
                // If the address id is an empty guid it means that a new empty address should be used. The only thing we
                // keep is the id of currently selected shipping method.
                updatedAddress = new ShippingAddress
                {
                    ShippingMethodId = updatedAddress.ShippingMethodId,
                    Name = _localizationService.GetString("/Shared/Address/NewAddress") + " " + viewModel.AvailableAddresses.Count
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

            return PartialView("Address", viewModel);
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
            List<ShippingAddress> addressCollection = new List<ShippingAddress>(viewModel.ShippingAddresses);
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
                }),
            };

            return PartialView(viewModel);
        }

        [HttpPost]
        public ActionResult Purchase(CheckoutPage currentPage, CheckoutViewModel checkoutViewModel, IPaymentMethodViewModel<IPaymentOption> paymentViewModel)
        {
            // Since the payment property is marked with an exclude binding attribute in the CheckoutViewModel
            // it needs to be manually re-added again.
            checkoutViewModel.Payment = paymentViewModel;

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
                InitializeCheckoutViewModel(currentPage, checkoutViewModel);
                return View("Index", checkoutViewModel);
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
                InitializeCheckoutViewModel(currentPage, checkoutViewModel);
                return View("Index", checkoutViewModel);
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
                InitializeCheckoutViewModel(currentPage, checkoutViewModel);
                return View("Index", checkoutViewModel);
            }

            return Finish(currentPage);
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

            foreach (ShippingAddress shippingAddress in checkoutViewModel.ShippingAddresses)
            {
                var orderAddress = _checkoutService.AddNewOrderAddress();
                _addressBookService.MapModelToOrderAddress(shippingAddress, orderAddress);
                orderAddress.Name = Guid.NewGuid().ToString();
                orderAddress.AcceptChanges();

                var shipment = _checkoutService.CreateShipment();
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

            return Redirect(new UrlBuilder(confirmationPage.LinkURL) { QueryCollection = queryCollection }.ToString());
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            _controllerExceptionHandler.HandleRequestValidationException(filterContext, "purchase", OnPurchaseException);
        }

        public ActionResult OnPurchaseException(ExceptionContext filterContext)
        {
            var currentPage = filterContext.RequestContext.GetRoutedData<CheckoutPage>();
            if (currentPage == null)
            {
                return new EmptyResult();
            }

            CheckoutViewModel viewModel = InitializeCheckoutViewModel(currentPage, null);
            ModelState.AddModelError("Purchase", filterContext.Exception.Message);

            return View("index", viewModel);
        }

    }
}
