using Mediachase.Commerce.Website;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.Models
{
    public interface IPaymentMethodViewModel<out T> where T : IPaymentOption
    {
        T PaymentMethod { get; }
        string Description { get; set; }
        string SystemName { get; set; }
    }
}
