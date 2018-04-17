using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Orders;
using System;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods
{
    [ServiceConfiguration(typeof(IPaymentMethod))]
    public class AuthorizePaymentMethod : CreditCardPaymentMethodBase
    {
        public override string SystemKeyword => "Authorize";

        public override void InitializeValues()
        {
            base.InitializeValues();

            ExpirationMonth = DateTime.Now.AddMonths(1).Month;
            ExpirationYear = DateTime.Now.AddMonths(1).Year;
            CreditCardSecurityCode = "027";
            CreditCardNumber = "4007000000027";
        }

        public override IPayment CreatePayment(decimal amount, IOrderGroup orderGroup)
        {
            var payment = orderGroup.CreateCardPayment(OrderGroupFactory);
            payment.CardType = "Credit card";
            payment.PaymentMethodId = PaymentMethodId;
            payment.PaymentMethodName = SystemKeyword;
            payment.Amount = amount;
            payment.CreditCardNumber = CreditCardNumber;
            payment.CreditCardSecurityCode = CreditCardSecurityCode;
            payment.ExpirationMonth = ExpirationMonth;
            payment.ExpirationYear = ExpirationYear;
            payment.Status = PaymentStatus.Pending.ToString();
            payment.CustomerName = CreditCardName;
            return payment;
        }
    }
}