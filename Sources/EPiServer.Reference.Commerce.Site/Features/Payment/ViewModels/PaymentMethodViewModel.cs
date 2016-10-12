using System;
using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels
{
    public class PaymentMethodViewModel<T> : IPaymentMethodViewModel<T> where T : PaymentMethodBase
    {
        public Guid PaymentMethodId { get; set; }
        public string SystemName { get; set; }
        public string FriendlyName { get; set; }
        public string Description { get; set; }
        public T PaymentMethod { get; set; }
    }
}