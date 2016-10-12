using System;
using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;
using EPiServer.ServiceLocation;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels
{
    public class PaymentMethodViewModelResolver
    {
        private static readonly Lazy<IOrderFactory> _orderFactory = new Lazy<IOrderFactory>(() => ServiceLocator.Current.GetInstance<IOrderFactory>());

        public static IPaymentMethodViewModel<PaymentMethodBase> Resolve(string paymentMethodName, IOrderFactory orderFactory)
        {
            switch (paymentMethodName)
            {
                case "CashOnDelivery":
                    return new CashOnDeliveryViewModel { PaymentMethod = new CashOnDeliveryPaymentMethod(orderFactory)};
                case "GenericCreditCard":
                    return new GenericCreditCardViewModel { PaymentMethod = new GenericCreditCardPaymentMethod(orderFactory) };
            }

            throw new ArgumentException("No view model has been implemented for the method " + paymentMethodName, "paymentMethodName");
        }

        public static IPaymentMethodViewModel<PaymentMethodBase> Resolve(string paymentMethodName)
        {
            return Resolve(paymentMethodName, _orderFactory.Value);
        }
    }
}