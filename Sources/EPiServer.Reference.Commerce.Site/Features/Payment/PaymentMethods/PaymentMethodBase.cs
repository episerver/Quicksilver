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

        /// <summary>
        /// Gets or sets the name of the payment method.
        /// </summary>
        /// <remarks>
        /// The name property should be overriden in any subclass because this value is later
        /// used to determine which controller and view should be used for renderring. It is
        /// also used by the PaymentModelBinder
        /// </remarks>
        public virtual string Name
        {
            get { return string.Empty; }
        }
    }
}