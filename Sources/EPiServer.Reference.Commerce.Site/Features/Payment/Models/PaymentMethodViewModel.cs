using System;
using Mediachase.Commerce;
using Mediachase.Commerce.Website;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.Models
{
    /// <summary>
    /// Generic base class for a payment method view model.
    /// </summary>
    /// <typeparam name="T">The type of the related payment method.</typeparam>
    public class PaymentMethodViewModel<T> : IPaymentMethodViewModel<T> where T : IPaymentOption
    {
        public Guid Id { get; set; }
        public string SystemName { get; set; }
        public string FriendlyName { get; set; }
        public string Description { get; set; }
        public MarketId MarketId { get; set; }
        public int Ordering { get; set; }
        public bool IsDefault { get; set; }
        public T PaymentMethod { get; set; }
        public virtual string Controller { get; set; }
    }
}