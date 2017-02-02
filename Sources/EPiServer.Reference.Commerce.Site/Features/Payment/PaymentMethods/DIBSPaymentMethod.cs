using EPiServer.Commerce.Order;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Orders;
using System.ComponentModel;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods
{
    public class DIBSPaymentMethod : PaymentMethodBase
    {
        public DIBSPaymentMethod()
            : this(LocalizationService.Current, ServiceLocator.Current.GetInstance<IOrderGroupFactory>())
        {
        }

        public DIBSPaymentMethod(LocalizationService localizationService, IOrderGroupFactory orderGroupFactory)
            : base(localizationService, orderGroupFactory)
        {
        }

        public string Error
        {
            get { return null; }
        }

        public override IPayment CreatePayment(decimal amount, IOrderGroup orderGroup)
        {
            var payment = orderGroup.CreatePayment(_orderGroupFactory);
            payment.PaymentMethodId = PaymentMethodId;
            payment.PaymentMethodName = "DIBS";
            payment.Amount = amount;
            payment.Status = PaymentStatus.Pending.ToString();
            payment.TransactionType = TransactionType.Authorization.ToString();
            return payment;
        }

        public override void PostProcess(IPayment payment)
        {

        }

        public override bool ValidateData()
        {
            return true;
        }
    }
}