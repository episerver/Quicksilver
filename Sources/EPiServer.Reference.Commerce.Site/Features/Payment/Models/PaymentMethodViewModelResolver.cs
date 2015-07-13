using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;
using Mediachase.Commerce.Website;
using System;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.Models
{
    public class PaymentMethodViewModelResolver
    {
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