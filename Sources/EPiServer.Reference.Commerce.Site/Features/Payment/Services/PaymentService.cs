using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.Payment.Exceptions;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Website;
using Mediachase.Commerce.Website.Helpers;
using System;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.Services
{
    /// <summary>
    /// Service for processing a payment transaction.
    /// </summary>
    [ServiceConfiguration(typeof(IPaymentService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class PaymentService : IPaymentService
    {
        private readonly Func<CartHelper> _cartHelper;
        private readonly LocalizationService _localizationService;

        public PaymentService(Func<CartHelper> cartHelper, LocalizationService localizationService)
        {
            _cartHelper = cartHelper;
            _localizationService = localizationService;
        }

        /// <summary>
        /// Processes the provided payment.
        /// </summary>
        /// <param name="method">The payment to process.</param>
        /// <returns>Service result indicating whether the processing was successfull or not.</returns>
        public void ProcessPayment(IPaymentOption method)
        {
            var cart = _cartHelper().Cart;

            if (!cart.OrderForms.Any())
            {
                cart.OrderForms.AddNew();
            }

            var payment = method.PreProcess(cart.OrderForms[0]);

            if (payment == null)
            {
                throw new PreProcessException();
            }

            cart.OrderForms[0].Payments.Add(payment);
            cart.AcceptChanges();

            method.PostProcess(cart.OrderForms[0]);
        }
    }
}