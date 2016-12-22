using EPiServer.Framework.Localization;
using System;
using EPiServer.Commerce.Order;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods
{
    public abstract class PaymentMethodBase
    {
        protected readonly LocalizationService _localizationService;
        protected readonly IOrderGroupFactory _orderGroupFactory;

        protected PaymentMethodBase(LocalizationService localizationService, IOrderGroupFactory orderGroupFactory)
        {
            _localizationService = localizationService;
            _orderGroupFactory = orderGroupFactory;
        }

        public Guid PaymentMethodId { get; set; }

        public abstract IPayment CreatePayment(decimal amount, IOrderGroup orderGroup);

        public abstract void PostProcess(IPayment payment);

        public abstract bool ValidateData();
    }
}