using EPiServer.Commerce.Order;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Plugins.Payment;
using System;

namespace EPiServer.Reference.Commerce.Shared
{
    public class GenericCreditCardPaymentGateway : AbstractPaymentGateway, IPaymentPlugin
    {
        /// <inheritdoc/>
        public override bool ProcessPayment(Payment payment, ref string message)
        {
            var paymentProcessingResult = ProcessPayment(payment.Parent.Parent, payment);
            message = paymentProcessingResult.Message;
            return paymentProcessingResult.IsSuccessful;
        }

        /// <summary>
        /// Processes the payment. Can be used for both positive and negative transactions.
        /// </summary>
        /// <param name="orderGroup">The order group.</param>
        /// <param name="payment">The payment.</param>
        /// <returns>The payment processing result.</returns>
        public PaymentProcessingResult ProcessPayment(IOrderGroup orderGroup, IPayment payment)
        {
            var creditCardPayment = (ICreditCardPayment)payment;

            if (creditCardPayment.ExpirationYear < DateTime.Now.Year || 
                (creditCardPayment.ExpirationYear == DateTime.Now.Year && creditCardPayment.ExpirationMonth < DateTime.Now.Month))
            {
                return PaymentProcessingResult.CreateUnsuccessfulResult("Credit card was expired.");
            }

            return creditCardPayment.CreditCardNumber.EndsWith("4")
                ? PaymentProcessingResult.CreateSuccessfulResult(string.Empty)
                : PaymentProcessingResult.CreateUnsuccessfulResult("Invalid credit card number.");
        }
    }
}