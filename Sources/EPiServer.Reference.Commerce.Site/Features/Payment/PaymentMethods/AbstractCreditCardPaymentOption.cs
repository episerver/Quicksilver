using System;
using System.Linq;
using EPiServer.Reference.Commerce.Site.Features.Payment.Models;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Website;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods
{
    public class AbstractCreditCardPaymentOption : IPaymentOption
    {   
        private readonly AbstractCreditCardPaymentViewModel _payment;

        public AbstractCreditCardPaymentOption(AbstractCreditCardPaymentViewModel payment)
        {
            _payment = payment;
        }

        public bool ValidateData()
        {
            if (string.IsNullOrEmpty(_payment.CreditCardNumber))
                return false;

            if (_payment.CreditCardNumber[_payment.CreditCardNumber.Length - 1] != '4')
                return false;

            if (string.IsNullOrEmpty(_payment.CreditCardSecurityCode))
                return false;
            if (_payment.ExpirationYear < DateTime.Now.Year)
                return false;
            if (_payment.ExpirationYear == DateTime.Now.Year && _payment.ExpirationMonth < DateTime.Now.Month)
                return false;

            return true;
        }

        public Mediachase.Commerce.Orders.Payment PreProcess(OrderForm orderForm)
        {
            if (orderForm == null) throw new ArgumentNullException("orderForm");

            if (!ValidateData())
                return null;

            var payment = new CreditCardPayment
                {
                    CardType = "VISA",
                    PaymentMethodId = _payment.PaymentMethodId,
                    PaymentMethodName = "CreditCardPayment",
                    OrderFormId = orderForm.OrderFormId,
                    OrderGroupId = orderForm.OrderGroupId,
                    Amount = orderForm.Total,
                    CreditCardNumber = _payment.CreditCardNumber,
                    CreditCardSecurityCode = _payment.CreditCardSecurityCode,
                    ExpirationMonth = _payment.ExpirationMonth,
                    ExpirationYear = _payment.ExpirationYear,
                    Status = PaymentStatus.Pending.ToString(),
                    CustomerName = _payment.CreditCardName
                };

            return payment;
        }

        public bool PostProcess(OrderForm orderForm)
        {
            var card = orderForm.Payments.ToArray().FirstOrDefault(x => x.PaymentType == PaymentType.CreditCard);
            if (card == null)
                return false;

            card.Status = PaymentStatus.Processed.ToString();
            card.AcceptChanges();
            return true;
        }
    }
}