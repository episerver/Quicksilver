using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.Payment.Exceptions;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Website;
using Mediachase.Commerce.Website.Helpers;
using System;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.Services
{
    [ServiceConfiguration(typeof(IPaymentService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class PaymentService : IPaymentService
    {
        private readonly Func<string, CartHelper> _cartHelper;
        private readonly LocalizationService _localizationService;

        public PaymentService(Func<string, CartHelper> cartHelper, LocalizationService localizationService)
        {
            _cartHelper = cartHelper;
            _localizationService = localizationService;
        }

        public void ProcessPayment(IPaymentOption method)
        {
            var cart = _cartHelper(Mediachase.Commerce.Orders.Cart.DefaultName).Cart;

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