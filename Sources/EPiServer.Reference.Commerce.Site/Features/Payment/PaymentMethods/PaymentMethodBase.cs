using EPiServer.Framework.Localization;
using System;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods
{
    /// <summary>
    /// Base class for all payment methods.
    /// </summary>
    public abstract class PaymentMethodBase
    {
        protected readonly LocalizationService _localizationService;

        public PaymentMethodBase(LocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        /// <summary>
        /// Gets or sets the ID of the payment method.
        /// </summary>
        public Guid PaymentMethodId { get; set; }

    }
}