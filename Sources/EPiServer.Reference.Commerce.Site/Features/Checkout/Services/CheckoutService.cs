using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Logging;
using EPiServer.Reference.Commerce.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Services
{
    public class CheckoutService
    {
        private readonly IAddressBookService _addressBookService;
        private readonly IOrderGroupCalculator _orderGroupCalculator;
        private readonly IOrderGroupFactory _orderGroupFactory;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly IOrderRepository _orderRepository;
        private readonly IContentRepository _contentRepository;
        private readonly CustomerContextFacade _customerContext;
        private readonly LocalizationService _localizationService;
        private readonly IMailService _mailService;
        private readonly IPromotionEngine _promotionEngine;
        private readonly ILogger _log = LogManager.GetLogger(typeof(CheckoutService));
        private readonly ICartService _cartService;

        public AuthenticatedPurchaseValidation AuthenticatedPurchaseValidation { get; private set; }
        public AnonymousPurchaseValidation AnonymousPurchaseValidation { get; private set; }
        public CheckoutAddressHandling CheckoutAddressHandling { get; private set; }

        public CheckoutService(
            IAddressBookService addressBookService,
            IOrderGroupFactory orderGroupFactory,
            IOrderGroupCalculator orderGroupCalculator,
            IPaymentProcessor paymentProcessor,
            IOrderRepository orderRepository,
            IContentRepository contentRepository,
            CustomerContextFacade customerContext,
            LocalizationService localizationService,
            IMailService mailService, 
            IPromotionEngine promotionEngine,
            ICartService cartService)
        {
            _addressBookService = addressBookService;
            _orderGroupFactory = orderGroupFactory;
            _orderGroupCalculator = orderGroupCalculator;
            _paymentProcessor = paymentProcessor;
            _orderRepository = orderRepository;
            _contentRepository = contentRepository;
            _customerContext = customerContext;
            _localizationService = localizationService;
            _mailService = mailService;
            _promotionEngine = promotionEngine;
            _cartService = cartService;

            AuthenticatedPurchaseValidation = new AuthenticatedPurchaseValidation(_localizationService);
            AnonymousPurchaseValidation = new AnonymousPurchaseValidation(_localizationService);
            CheckoutAddressHandling = new CheckoutAddressHandling(_addressBookService);
        }

        public virtual void UpdateShippingMethods(ICart cart, IList<ShipmentViewModel> shipmentViewModels)
        {
            var index = 0;
            foreach (var shipment in cart.GetFirstForm().Shipments)
            {
                shipment.ShippingMethodId = shipmentViewModels[index++].ShippingMethodId;
            }
        }
        
        public virtual void UpdateShippingAddresses(ICart cart, CheckoutViewModel viewModel)
        {
            if (viewModel.UseBillingAddressForShipment)
            {
                cart.GetFirstShipment().ShippingAddress = _addressBookService.ConvertToAddress(viewModel.BillingAddress, cart);
            }
            else
            {
                var shipments = cart.GetFirstForm().Shipments;
                for (var index = 0; index < shipments.Count; index++)
                {
                    shipments.ElementAt(index).ShippingAddress = _addressBookService.ConvertToAddress(viewModel.Shipments[index].Address, cart);
                }
            }
        }

        public virtual void ApplyDiscounts(ICart cart)
        {
            cart.ApplyDiscounts(_promotionEngine, new PromotionEngineSettings());
        }

        public virtual void CreateAndAddPaymentToCart(ICart cart, CheckoutViewModel viewModel)
        {
            // Clean up payments in cart on payment provider site.
            foreach (IOrderForm form in cart.Forms)
            {
                form.Payments.Clear();
            }

            var total = cart.GetTotal(_orderGroupCalculator);
            var payment = viewModel.Payment.CreatePayment(total.Amount, cart);
            cart.AddPayment(payment, _orderGroupFactory);
            payment.BillingAddress = _addressBookService.ConvertToAddress(viewModel.BillingAddress, cart);
        }

        public virtual IPurchaseOrder PlaceOrder(ICart cart, ModelStateDictionary modelState, CheckoutViewModel checkoutViewModel)
        {
            try
            {
                var paymentProcessingResults = cart.ProcessPayments(_paymentProcessor, _orderGroupCalculator);

                if (paymentProcessingResults.Any(r => !r.IsSuccessful))
                {
                    modelState.AddModelError("", _localizationService.GetString("/Checkout/Payment/Errors/ProcessingPaymentFailure") + string.Join(", ", paymentProcessingResults.Select(p => p.Message)));
                    return null;
                }

                var redirectPayment = paymentProcessingResults.FirstOrDefault(r => !string.IsNullOrEmpty(r.RedirectUrl));
                if (redirectPayment != null)
                {
                    checkoutViewModel.RedirectUrl = redirectPayment.RedirectUrl;
                    return null;
                }

                var processedPayments = cart.GetFirstForm().Payments.Where(x => x.Status.Equals(PaymentStatus.Processed.ToString()));
                if (!processedPayments.Any())
                {
                    // Return null in case there is no payment was processed.
                    return null;
                }

                var totalProcessedAmount = processedPayments.Sum(x => x.Amount);
                if (totalProcessedAmount != cart.GetTotal(_orderGroupCalculator).Amount)
                {
                    throw new InvalidOperationException("Wrong amount");
                }

                PurchaseValidation validation = null;
                if (checkoutViewModel.IsAuthenticated)
                {
                    validation = AuthenticatedPurchaseValidation;
                }
                else
                {
                    validation = AnonymousPurchaseValidation;
                }

                if (!validation.ValidateOrderOperation(modelState,  _cartService.RequestInventory(cart)))
                {
                    return null;
                }

                var orderReference = _orderRepository.SaveAsPurchaseOrder(cart);
                var purchaseOrder = _orderRepository.Load<IPurchaseOrder>(orderReference.OrderGroupId);
                _orderRepository.Delete(cart.OrderLink);

                return purchaseOrder;
            }
            catch (PaymentException ex)
            {
                modelState.AddModelError("", _localizationService.GetString("/Checkout/Payment/Errors/ProcessingPaymentFailure") + ex.Message);
            }
            return null;
        }

        public virtual bool SendConfirmation(CheckoutViewModel viewModel, IPurchaseOrder purchaseOrder)
        {
            var queryCollection = new NameValueCollection
            {
                {"contactId", _customerContext.CurrentContactId.ToString()},
                {"orderNumber", purchaseOrder.OrderLink.OrderGroupId.ToString(CultureInfo.CurrentCulture)}
            };

            var startpage = _contentRepository.Get<StartPage>(ContentReference.StartPage);
            var confirmationPage = _contentRepository.GetChildren<OrderConfirmationPage>(viewModel.CurrentPage.ContentLink).FirstOrDefault();

            try
            {
                _mailService.Send(startpage.OrderConfirmationMail, queryCollection, viewModel.BillingAddress.Email, confirmationPage.Language.Name);
            }
            catch (Exception e)
            {
                _log.Warning(string.Format("Unable to send purchase receipt to '{0}'.", viewModel.BillingAddress.Email), e);
                return false;
            }
            return true;
        }

        public virtual string BuildRedirectionUrl(CheckoutViewModel checkoutViewModel, IPurchaseOrder purchaseOrder, bool confirmationSentSuccessfully)
        {
            var queryCollection = new NameValueCollection
            {
                {"contactId", _customerContext.CurrentContactId.ToString()},
                {"orderNumber", purchaseOrder.OrderLink.OrderGroupId.ToString(CultureInfo.CurrentCulture)}
            };

            if (!confirmationSentSuccessfully)
            {
                queryCollection.Add("notificationMessage", string.Format(_localizationService.GetString("/OrderConfirmationMail/ErrorMessages/SmtpFailure"), checkoutViewModel.BillingAddress.Email));
            }

            var confirmationPage = _contentRepository.GetChildren<OrderConfirmationPage>(checkoutViewModel.CurrentPage.ContentLink).FirstOrDefault();

            return new UrlBuilder(confirmationPage.LinkURL) {QueryCollection = queryCollection}.ToString();
        }

        public virtual bool ValidateOrder(ModelStateDictionary modelState, CheckoutViewModel viewModel, Dictionary<ILineItem, List<ValidationIssue>> validationIssueCollections)
        {
            PurchaseValidation validation = null;
            if (viewModel.IsAuthenticated)
            {
                validation = AuthenticatedPurchaseValidation;
            }
            else
            {
                validation = AnonymousPurchaseValidation;
            }

            if (!validation.ValidateModel(modelState, viewModel) ||
                !validation.ValidateOrderOperation(modelState, validationIssueCollections))
            {
                return false;
            }

            return true;
        }

        public void ProcessPaymentCancel(CheckoutViewModel viewModel, TempDataDictionary tempData, ControllerContext controlerContext)
        {
            var message = tempData["message"] != null ? tempData["message"].ToString() : controlerContext.HttpContext.Request.QueryString["message"];
            if (!string.IsNullOrEmpty(message))
            {
                viewModel.Message = message;
            }
        }
    }
}