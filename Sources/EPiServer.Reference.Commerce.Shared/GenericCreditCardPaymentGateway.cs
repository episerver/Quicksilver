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
            return ProcessPayment(payment as IPayment, ref message);
        }

        /// <summary>
        /// Processes the payment. Can be used for both positive and negative transactions.
        /// </summary>
        /// <param name="payment">The payment.</param>
        /// <param name="message">The message.</param>
        /// <returns><c>True</c> if payment processed successfully, otherwise <c>False</c></returns>
        public bool ProcessPayment(IPayment payment, ref string message)
        {
            var creditCardPayment = (ICreditCardPayment)payment;

            if (!creditCardPayment.CreditCardNumber.EndsWith("4"))
            {
                message = "Invalid credit card number.";
                return false;
            }

            return true;
        }
    }
}