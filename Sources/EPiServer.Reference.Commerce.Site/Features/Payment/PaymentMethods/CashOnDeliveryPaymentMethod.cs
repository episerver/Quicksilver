using EPiServer.Commerce.Order;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.Services;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Orders;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods
{
    [ServiceConfiguration(typeof(IPaymentMethod))]
    public class CashOnDeliveryPaymentMethod : PaymentMethodBase
    {
        public override string SystemKeyword => "CashOnDelivery";

        public CashOnDeliveryPaymentMethod() 
            : this(LocalizationService.Current, 
                  ServiceLocator.Current.GetInstance<IOrderGroupFactory>(),
                  ServiceLocator.Current.GetInstance<LanguageService>(),
                  ServiceLocator.Current.GetInstance<IPaymentManagerFacade>())
        {
        }

        public CashOnDeliveryPaymentMethod(LocalizationService localizationService,
            IOrderGroupFactory orderGroupFactory,
            LanguageService languageService,
            IPaymentManagerFacade paymentManager)
            : base(localizationService, orderGroupFactory, languageService, paymentManager)
        {
        }

        public override bool ValidateData()
        {
            return true;
        }
        
        public override IPayment CreatePayment(decimal amount, IOrderGroup orderGroup)
        {
            var payment = orderGroup.CreatePayment(OrderGroupFactory);
            payment.PaymentType = PaymentType.Other;
            payment.PaymentMethodId = PaymentMethodId;
            payment.PaymentMethodName = SystemKeyword;
            payment.Amount = amount;
            payment.Status = PaymentStatus.Pending.ToString();
            payment.TransactionType = TransactionType.Sale.ToString();
            return payment;
        }
    }
}