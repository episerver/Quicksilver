using EPiServer.Commerce.Order;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Plugins.Payment;

namespace EPiServer.Reference.Commerce.Shared
{
    public class GenericCreditCardPaymentGateway : AbstractPaymentGateway, IPaymentPlugin
    {
        /// <inheritdoc/>
        public override bool ProcessPayment(Payment payment, ref string message)
        {
            var creditCardPayment = (CreditCardPayment)payment;

            if (!creditCardPayment.CreditCardNumber.EndsWith("4"))
            {
                message = "Invalid credit card number.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Processes the payment. Can be used for both positive and negative transactions.
        /// </summary>
        /// <param name="payment">The payment.</param>
        /// <param name="message">The message.</param>
        /// <returns><c>True</c> if payment processed successfully, otherwise <c>False</c></returns>
        bool IPaymentPlugin.ProcessPayment(IPayment payment, ref string message)
        {
            return ProcessPayment((Payment)payment, ref message);
        }
    }
}