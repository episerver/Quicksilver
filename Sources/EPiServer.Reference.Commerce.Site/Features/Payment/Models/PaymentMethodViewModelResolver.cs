using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;
using Mediachase.Commerce.Website;
using System;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.Models
{
    public class PaymentMethodViewModelResolver
    {
        /// <summary>
        /// Provides an instance of a view model dependent on the name of a payment method.
        /// </summary>
        /// <param name="paymentMethodName">The name used to identify what view model should be returned.</param>
        /// <returns>A new instance of a PaymentMethodViewModel.</returns>
        public static IPaymentMethodViewModel<IPaymentOption> Resolve(string paymentMethodName)
        {
            switch (paymentMethodName)
            {
                case "CashOnDelivery":
                    return new CashOnDeliveryViewModel() { PaymentMethod = new CashOnDeliveryPaymentMethod() };

                case "GenericCreditCard":
                    return new GenericCreditCardViewModel() { PaymentMethod = new GenericCreditCardPaymentMethod() };
            }

            throw new ArgumentException("No view model has been implemented for the method " + paymentMethodName, "paymentMethodName");
        }
    }
}