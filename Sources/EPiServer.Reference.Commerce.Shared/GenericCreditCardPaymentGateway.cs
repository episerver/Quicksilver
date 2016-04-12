using Mediachase.Commerce.Plugins.Payment;

namespace EPiServer.Reference.Commerce.Shared
{
    public class GenericCreditCardPaymentGateway : AbstractPaymentGateway
    {
        /// <inheritdoc/>
        public override bool ProcessPayment(Mediachase.Commerce.Orders.Payment payment, ref string message)
        {
            //Simply accept the payment gateway.
            return true;
        }
    }
}
