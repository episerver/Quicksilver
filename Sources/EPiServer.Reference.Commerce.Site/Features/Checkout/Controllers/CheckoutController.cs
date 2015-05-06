using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Models;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
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
        private readonly Func<CartHelper> _cartHelper;
        private readonly IContentRepository _contentRepository;
        private readonly UrlResolver _urlResolver;
        private readonly IMailSender _mailSender;
        private readonly ICheckoutService _checkoutService;
        private readonly IContentLoader _contentLoader;

        public CheckoutController(Func<CartHelper> cartHelper, IContentRepository contentRepository, UrlResolver urlResolver, IMailSender mailSender,
                                  ICheckoutService checkoutService, IContentLoader contentLoader)
        {
            _cartHelper = cartHelper;
            _contentRepository = contentRepository;
            _urlResolver = urlResolver;
            _mailSender = mailSender;
            _checkoutService = checkoutService;
            _contentLoader = contentLoader;
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
            var shipment = _checkoutService.CreateShipment();
            var shippingRates = _checkoutService.GetShippingRates(shipment);
            var selectedShippingRate = shippingRates.First();
            var customer = CustomerContext.Current.CurrentContact;

            _checkoutService.UpdateShipment(shipment, selectedShippingRate);
            _cartHelper().RunWorkflow(OrderGroupWorkflowManager.CartValidateWorkflowName);
            Cart.AcceptChanges();

            var paymentMethods = _checkoutService.GetPaymentMethods();
            var selectedPaymentMethod = paymentMethods.First();
            var preferredShippingAddress = customer != null ? customer.PreferredShippingAddress : null;

            var formModel = new CheckoutFormModel
            {
                SelectedShippingMethodId = selectedShippingRate.Id,
                SelectedPaymentMethodId = selectedPaymentMethod.Id,
                AddressFormModel = _checkoutService.MapAddressToAddressForm(preferredShippingAddress)
            };

            var model = new CheckoutViewModel
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

            return View(model);
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
        public ActionResult Update(CheckoutFormModel formModel)
        {
            var paymentMethods = _checkoutService.GetPaymentMethods().OrderBy(x => x.IsDefault).ThenBy(x => x.Ordering);
            var selectedPaymentMethod = paymentMethods.FirstOrDefault(x => x.Id == formModel.SelectedPaymentMethodId) ?? paymentMethods.First();

            var shipment = _checkoutService.CreateShipment();
            var shippingMethods = _checkoutService.GetShippingRates(shipment);
            var selectedShippingMethod = shippingMethods.FirstOrDefault(x => x.Id == formModel.SelectedShippingMethodId) ?? shippingMethods.First();

            _checkoutService.UpdateShipment(shipment, selectedShippingMethod);
            _cartHelper().RunWorkflow(OrderGroupWorkflowManager.CartValidateWorkflowName);
            Cart.AcceptChanges();

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
            var orderForm = Cart.OrderForms.Count == 0 ? new [] { new OrderForm() } : Cart.OrderForms.ToArray();
            var currency = new Currency(Cart.BillingCurrency);
            return PartialView(new OrderSummaryViewModel
            {
                SubTotal = new Money(Cart.SubTotal + orderForm.SelectMany(x => x.Discounts.Cast<OrderFormDiscount>()).Sum(x => x.DiscountAmount), currency),
                CartTotal = new Money(Cart.Total, currency),
                ShippingTotal = new Money(Cart.ShippingTotal, currency),
                ShippingSubtotal = new Money(orderForm.SelectMany(x => x.Shipments).Sum(x => x.ShipmentTotal) + orderForm.SelectMany(x => x.Shipments).Sum(x => x.ShippingDiscountAmount), currency),
                OrderDiscounts = orderForm.SelectMany(x => x.Discounts.Cast<OrderFormDiscount>()).Select(x => new OrderDiscountModel
                {
                    Discount = new Money(x.DiscountAmount, currency),
                    Displayname = x.DisplayMessage
                }),
                OrderDiscountTotal = new Money(orderForm.SelectMany(x => x.Discounts.Cast<OrderFormDiscount>()).Sum(x => x.DiscountAmount), currency),
                ShippingDiscounts = orderForm.SelectMany(x => x.Shipments).SelectMany(x => x.Discounts.Cast<ShipmentDiscount>()).Select(x => new OrderDiscountModel
                {
                    Discount = new Money(x.DiscountAmount, currency),
                    Displayname = x.DisplayMessage
                }),
                ShippingDiscountTotal = new Money(orderForm.SelectMany(x => x.Shipments).SelectMany(x => x.Discounts.Cast<ShipmentDiscount>()).Sum(x => x.DiscountAmount), currency),
                ShippingTaxTotal = new Money(Cart.ShippingTotal + Cart.TaxTotal, currency),
                TaxTotal = new Money(Cart.TaxTotal, currency)
            });
        }

        [HttpPost]
        public ActionResult Purchase(CheckoutPage currentPage, CheckoutFormModel formModel)
        {
            Cart.OrderAddresses.Clear();
            var shippingAddress = Cart.OrderAddresses.AddNew();
            shippingAddress.FirstName = formModel.AddressFormModel.FirstName;
            shippingAddress.LastName = formModel.AddressFormModel.LastName;
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
            var shippingRate = _checkoutService.GetShippingRate(shipment, formModel.SelectedShippingMethodId);
            _checkoutService.UpdateShipment(shipment, shippingRate);

            Cart.AcceptChanges();

            return this.ExecuteAction("Process", formModel.PaymentViewModel.Controller, new
                {
                    Payment = formModel.PaymentViewModel
                });
        }

        [HttpGet]
        public ActionResult Finish(CheckoutPage currentPage)
        {
            var totalP = Cart.OrderForms[0].Payments.Where(x => x.Status.Equals(PaymentStatus.Processed.ToString())).Sum(x => x.Amount);
            if (totalP != Cart.OrderForms[0].Total)
            {
                throw new InvalidOperationException("Wrong amount");
            }

            var confirmationPage = _contentRepository.GetFirstChild<OrderConfirmationPage>(currentPage.ContentLink);
            var purchaseOrder = Cart.SaveAsPurchaseOrder();

            _checkoutService.DeleteCart();

            var startpage = _contentRepository.Get<StartPage>(ContentReference.StartPage);
            if (Request.Url != null)
            {
                _mailSender.Send(startpage.OrderConfirmationMail, new NameValueCollection
                    {
                        {"contactId", CustomerContext.Current.CurrentContactId.ToString()},
                        {"orderNumber", purchaseOrder.OrderGroupId.ToString(CultureInfo.InvariantCulture)}
                    }, purchaseOrder.OrderAddresses.First().Email);
            }
            return Redirect(new UrlBuilder(confirmationPage.LinkURL)
                {
                    QueryCollection = new NameValueCollection
                        {
                            {"ContactId", CustomerContext.Current.CurrentContactId.ToString()},
                            {"OrderNumber", purchaseOrder.OrderGroupId.ToString(CultureInfo.InvariantCulture)}
                        }
                }.ToString());
        }

        private Mediachase.Commerce.Orders.Cart Cart
        {
            get { return _cartHelper().Cart; }
        }
    }
}