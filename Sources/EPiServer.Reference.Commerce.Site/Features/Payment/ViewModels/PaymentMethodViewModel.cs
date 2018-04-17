using EPiServer.Commerce.Order;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels
{
    public class PaymentMethodViewModel<T> where T : IPaymentMethod
    {
        public T PaymentMethod { get; set; }

        public bool IsDefault { get; set; }
    }
}