using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.Cart;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Models;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Payment.Exceptions;
using EPiServer.Reference.Commerce.Site.Features.Payment.Models;
using EPiServer.Reference.Commerce.Site.Features.Payment.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
using Mediachase.BusinessFoundation.Data.Business;
using Mediachase.Commerce;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Website;
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
        private readonly IMailSender _mailSender;
        private readonly ICheckoutService _checkoutService;
        private readonly IPaymentService _paymentService;
        private readonly IContentLoader _contentLoader;
        private readonly LocalizationService _localizationService;


        public CheckoutController(ICartService cartService, IContentRepository contentRepository, UrlResolver urlResolver, IMailSender mailSender,
                                  ICheckoutService checkoutService, IContentLoader contentLoader, IPaymentService paymentService, LocalizationService localizationService)
        {
            _cartService = cartService;
            _contentRepository = contentRepository;
            _urlResolver = urlResolver;
            _mailSender = mailSender;
            _checkoutService = checkoutService;
            _paymentService = paymentService;
            _contentLoader = contentLoader;
            _localizationService = localizationService;
        }

        /// <summary>
        /// Renders the Checkout view.
        /// </summary>
        /// <param name="currentPage">The current content page.</param>
        /// <returns>The Checkout view.</returns>
        /// <remarks>If the user is logged in, her preferred shipping address will be
        /// used as default.</remarks>
        [HttpGet]
        public ActionResult Index(CheckoutPage currentPage)
        {
            CheckoutViewModel viewModel = CreateCheckoutViewModel(currentPage, null);

            return View(viewModel);
        }

        /// <summary>
        /// Creates a new instance of a CheckoutViewModel.
        /// </summary>
        /// <param name="currentPage">The current CheckoutPage.</param>
        /// <param name="formModel">Any previously used form values that should be re-used in the new view model.</param>
        /// <returns>A new CheckoutViewModel containing a CheckoutFormModel with an AddressFormModel and a PaymentViewModel.</returns>
        private CheckoutViewModel CreateCheckoutViewModel(CheckoutPage currentPage, CheckoutFormModel formModel)
        {
            var shipment = _checkoutService.CreateShipment();
            var shippingRates = _checkoutService.GetShippingRates(shipment);
            var selectedShippingRate = shippingRates.First();
            var customer = CustomerContext.Current.CurrentContact;
            var paymentMethods = _checkoutService.GetPaymentMethods();
            var selectedPaymentMethod = paymentMethods.First();
            var preferredShippingAddress = customer != null ? customer.PreferredShippingAddress : null;
            _checkoutService.UpdateShipment(shipment, selectedShippingRate);

            if (formModel == null)
            {

                _cartService.RunWorkflow(OrderGroupWorkflowManager.CartValidateWorkflowName);
                _cartService.SaveCart();

                formModel = new CheckoutFormModel
                {
                    SelectedShippingMethodId = selectedShippingRate.Id,
                    SelectedPaymentMethodId = selectedPaymentMethod.Id,
                    AddressFormModel = _checkoutService.MapAddressToAddressForm(preferredShippingAddress),
                    PaymentViewModel = new GenericCreditCardPaymentMethodViewModel
                    {
                        PaymentMethodId = selectedPaymentMethod.Id,
                        PaymentMethod = new Payment.PaymentMethods.GenericCreditCardPaymentMethod
                        {
                            PaymentMethodId = selectedPaymentMethod.Id,
                            ExpirationYear = DateTime.Today.Year,
                            ExpirationMonth = DateTime.Today.Month,
                            CardType = "MasterCard",
                            CreditCardNumber = "5555555555554444",
                            CreditCardSecurityCode = "123"
                        }
                    }
                };
            }

            var viewModel = new CheckoutViewModel
            {
                StartPage = _contentLoader.Get<StartPage>(ContentReference.StartPage),
                ReferrerUrl = GetReferrerUrl(),
                CurrentPage = currentPage,
                PaymentMethodViewModels = paymentMethods,
                SelectedPaymentMethodSystemName = selectedPaymentMethod.SystemName,
                ShippingMethodViewModels = GetShippingMethods(shippingRates),
                Addresses = GetContactAddresses(),
                CheckoutFormModel = formModel
            };

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

        private Dictionary<Guid, string> GetContactAddresses()
        {
            var addresses = CustomerContext.Current.CurrentContact != null
                    ? CustomerContext.Current.CurrentContact.ContactAddresses.ToDictionary<CustomerAddress, Guid, string>(
                        x => x.AddressId,
                        x => !string.IsNullOrEmpty(x.Name) ? x.Name : "Default")
                    : new Dictionary<Guid, string>();
            addresses.Add(Guid.Empty, "New Address");

            return addresses;
        }

        [HttpPost]
        public ActionResult Update(CheckoutFormModel formModel, [ModelBinder(typeof(PaymentViewModelBinder))] IPaymentMethodViewModel<IPaymentOption> paymentViewModel)
        {
            var paymentMethods = _checkoutService.GetPaymentMethods().OrderBy(x => x.IsDefault).ThenBy(x => x.Ordering);
            var selectedPaymentMethod = paymentMethods.FirstOrDefault(x => x.Id == formModel.SelectedPaymentMethodId) ?? paymentMethods.First();

            var shipment = _checkoutService.CreateShipment();
            var shippingMethods = _checkoutService.GetShippingRates(shipment);
            var selectedShippingMethod = shippingMethods.FirstOrDefault(x => x.Id == formModel.SelectedShippingMethodId) ?? shippingMethods.First();

            _checkoutService.UpdateShipment(shipment, selectedShippingMethod);
            _cartService.RunWorkflow(OrderGroupWorkflowManager.CartValidateWorkflowName);
            _cartService.SaveCart();

            formModel.PaymentViewModel = paymentViewModel;
            formModel.SelectedShippingMethodId = selectedShippingMethod.Id;
            formModel.SelectedPaymentMethodId = selectedPaymentMethod.Id;

            var model = new CheckoutViewModel
                {
                    StartPage = _contentLoader.Get<StartPage>(ContentReference.StartPage),
                    PaymentMethodViewModels = paymentMethods,
                    SelectedPaymentMethodSystemName = selectedPaymentMethod.SystemName,
                    ShippingMethodViewModels = GetShippingMethods(shippingMethods),
                    CheckoutFormModel = formModel
                };

            return PartialView("Partial", model);
        }

        [HttpPost]
        public ActionResult ChangeAddress(CheckoutFormModel formModel)
        {
            var addresses = CustomerContext.Current.CurrentContact.ContactAddresses.ToDictionary<CustomerAddress, Guid, string>(
                x => x.AddressId,
                x => !string.IsNullOrEmpty(x.Name) ? x.Name : "Default");
            addresses.Add(Guid.Empty, "New Address");

            var address = CustomerContext.Current.CurrentContact.ContactAddresses.FirstOrDefault(x => x.AddressId == formModel.AddressFormModel.AddressId)
                          ?? CustomerAddress.CreateInstance();

            formModel = new CheckoutFormModel
            {
                AddressFormModel = _checkoutService.MapAddressToAddressForm(address)
            };

            var model = new CheckoutViewModel
                {
                    StartPage = _contentLoader.Get<StartPage>(ContentReference.StartPage),
                    Addresses = addresses,
                    CheckoutFormModel = formModel
                };
            return PartialView("Address", model);
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
                    Discount = _cartService.ConvertToMoney(x.DiscountAmount),
                    Displayname = x.DisplayMessage
                }),
                ShippingDiscounts = orderForm.SelectMany(x => x.Shipments).SelectMany(x => x.Discounts.Cast<ShipmentDiscount>()).Select(x => new OrderDiscountModel
                {
                    Discount = _cartService.ConvertToMoney(x.DiscountAmount),
                    Displayname = x.DisplayMessage
                }),
            };

            return PartialView(viewModel);
        }

        [HttpPost]
        public ActionResult Purchase(CheckoutPage currentPage, CheckoutFormModel formModel, [ModelBinder(typeof(PaymentViewModelBinder))] IPaymentMethodViewModel<IPaymentOption> paymentViewModel)
        {
            formModel.PaymentViewModel = paymentViewModel;

            if (!ModelState.IsValid)
            {
                CheckoutViewModel viewModel = CreateCheckoutViewModel(currentPage, formModel);
                return View("Index", viewModel);
            }

            _checkoutService.ClearOrderAddresses();

            var shippingAddress = _checkoutService.AddNewOrderAddress();
            shippingAddress.FirstName = formModel.AddressFormModel.FirstName;
            shippingAddress.LastName = formModel.AddressFormModel.LastName;
            shippingAddress.Name = Guid.NewGuid().ToString();
            shippingAddress.Email = formModel.AddressFormModel.Email;
            shippingAddress.CountryName = formModel.AddressFormModel.Country;
            shippingAddress.Line1 = formModel.AddressFormModel.Address;
            shippingAddress.PostalCode = formModel.AddressFormModel.ZipCode;
            shippingAddress.City = formModel.AddressFormModel.City;
            shippingAddress.AcceptChanges();

            if (formModel.AddressFormModel.SaveAddress && User.Identity.IsAuthenticated)
            {
                var currentContact = CustomerContext.Current.CurrentContact;
                var address = currentContact.ContactAddresses.FirstOrDefault(x => x.AddressId == formModel.AddressFormModel.AddressId)
                              ?? CustomerAddress.CreateInstance();

                address.FirstName = formModel.AddressFormModel.FirstName;
                address.LastName = formModel.AddressFormModel.LastName;
                address.Email = formModel.AddressFormModel.Email;
                address.CountryName = formModel.AddressFormModel.Country;
                address.Line1 = formModel.AddressFormModel.Address;
                address.PostalCode = formModel.AddressFormModel.ZipCode;
                address.City = formModel.AddressFormModel.City;
                if (formModel.AddressFormModel.AddressId == Guid.Empty)
                {
                    currentContact.AddContactAddress(address);
                }
                else
                {
                    BusinessManager.Update(address);
                }
                currentContact.SaveChanges();
            }

            var shipment = _checkoutService.CreateShipment();
            shipment.ShippingAddressId = shippingAddress.Name;
            var shippingRate = _checkoutService.GetShippingRate(shipment, formModel.SelectedShippingMethodId);
            _checkoutService.UpdateShipment(shipment, shippingRate);

            _cartService.SaveCart();

            try
            {
                _paymentService.ProcessPayment(formModel.PaymentViewModel.PaymentMethod);
            }
            catch (PreProcessException)
            {
                ModelState.AddModelError("PaymentMethod", _localizationService.GetString("/Checkout/Payment/Errors/PreProcessingFailure"));
            }

            if (!ModelState.IsValid)
            {
                CheckoutViewModel viewModel = CreateCheckoutViewModel(currentPage, formModel);
                return View("Index", viewModel);
            }

            return Finish(currentPage);
        }

        [HttpGet]
        public ActionResult Finish(CheckoutPage currentPage)
        {
            if (!_cartService.GetAllLineItems().Any())
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

            purchaseOrder = _checkoutService.SaveCartAsPurchaseOrder();

            _checkoutService.DeleteCart();

            emailAddress = purchaseOrder.OrderAddresses.First().Email;

            NameValueCollection queryCollection = new NameValueCollection
            {
                {"contactId", CustomerContext.Current.CurrentContactId.ToString()},
                {"orderNumber", purchaseOrder.OrderGroupId.ToString(CultureInfo.InvariantCulture)}
            };

            try
            {
                _mailSender.Send(startpage.OrderConfirmationMail, queryCollection, emailAddress);
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
    }
}