using EPiServer.Commerce.Order;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Orders;
using System;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods
{
    [ServiceConfiguration(typeof(IPaymentMethod))]
    public class GenericCreditCardPaymentMethod : CreditCardPaymentMethodBase
    {
        public override string SystemKeyword => "GenericCreditCard";

        public override void InitializeValues()
        {
            base.InitializeValues();

            ExpirationMonth = DateTime.Now.AddMonths(1).Month;
            ExpirationYear = DateTime.Now.AddMonths(1).Year;
            CreditCardSecurityCode = "212";
            CardType = "Generic";
            CreditCardNumber = "4662519843660534";
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
            payment.TransactionType = TransactionType.Authorization.ToString();
            return payment;
        }

        protected override string ValidateCreditCardNumber()
        {
            if (string.IsNullOrEmpty(CreditCardNumber))
            {
                return LocalizationService.GetString("/Checkout/Payment/Methods/CreditCard/Empty/CreditCardNumber");
            }

            return CreditCardNumber[CreditCardNumber.Length - 1] != '4' ? 
                LocalizationService.GetString("/Checkout/Payment/Methods/CreditCard/ValidationErrors/CreditCardNumber") : 
                null;
        }
    }
}