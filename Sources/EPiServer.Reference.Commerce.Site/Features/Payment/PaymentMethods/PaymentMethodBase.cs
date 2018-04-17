using EPiServer.Commerce.Order;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.Services;
using System;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods
{
    public abstract class PaymentMethodBase : IPaymentMethod
    {
        protected readonly LocalizationService LocalizationService;
        protected readonly IOrderGroupFactory OrderGroupFactory;

        public Guid PaymentMethodId { get; }

        public abstract string SystemKeyword { get; }

        public string Name { get; }

        public string Description { get; }

        protected PaymentMethodBase(LocalizationService localizationService,
            IOrderGroupFactory orderGroupFactory,
            LanguageService languageService,
            IPaymentManagerFacade paymentManager)
        {
            LocalizationService = localizationService;
            OrderGroupFactory = orderGroupFactory;

            if (!string.IsNullOrEmpty(SystemKeyword))
            {
                var currentLanguage = languageService.GetCurrentLanguage().TwoLetterISOLanguageName;
                var dto = paymentManager.GetPaymentMethodBySystemName(SystemKeyword, currentLanguage);
                var paymentMethod = dto?.PaymentMethod?.FirstOrDefault();
                if (paymentMethod != null)
                {
                    PaymentMethodId = paymentMethod.PaymentMethodId;
                    Name = paymentMethod.Name;
                    Description = paymentMethod.Description;
                }
            }
        }

        public abstract IPayment CreatePayment(decimal amount, IOrderGroup orderGroup);

        public abstract bool ValidateData();
    }
}