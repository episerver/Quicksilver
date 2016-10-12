using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels
{
    public interface IPaymentMethodViewModel<out T> where T : PaymentMethodBase
    {
        T PaymentMethod { get; }
        string Description { get; set; }
        string SystemName { get; set; }
    }
}
