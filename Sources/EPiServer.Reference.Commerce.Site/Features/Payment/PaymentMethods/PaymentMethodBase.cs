using EPiServer.Framework.Localization;
using System;
using EPiServer.Commerce.Order;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods
{
    public abstract class PaymentMethodBase
    {
        protected readonly LocalizationService _localizationService;

        protected PaymentMethodBase(LocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public Guid PaymentMethodId { get; set; }

        public abstract IPayment CreatePayment(decimal amount);

        public abstract void PostProcess(IPayment payment);

        public abstract bool ValidateData();
    }
}