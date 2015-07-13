using EPiServer.Framework.Localization;
using System;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods
{
    public abstract class PaymentMethodBase
    {
        protected readonly LocalizationService _localizationService;

        public PaymentMethodBase(LocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public Guid PaymentMethodId { get; set; }

    }
}