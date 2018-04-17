using EPiServer.ServiceLocation;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.Services
{
    [ServiceConfiguration(typeof (IPaymentManagerFacade))]
    public class PaymentManagerFacade : IPaymentManagerFacade
    {
        public PaymentMethodDto GetPaymentMethodBySystemName(string name, string languageId)
        {
            return PaymentManager.GetPaymentMethodBySystemName(name, languageId);
        }

        public PaymentMethodDto GetPaymentMethodsByMarket(string marketId)
        {
            return PaymentManager.GetPaymentMethodsByMarket(marketId);
        }

        public void SavePaymentMethod(PaymentMethodDto dto)
        {
            PaymentManager.SavePayment(dto);
        }
    }
}