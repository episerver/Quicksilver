using EPiServer.Commerce.Order;
using Mediachase.Commerce.Orders;
using Newtonsoft.Json;
using System.Collections;

namespace EPiServer.Commerce.Orders.Cloud.Client.OrderModel
{
    [JsonConverter(typeof(PaymentConverter))]
    public class CloudyCreditCardPayment : CloudyPayment, ICreditCardPayment
    {
        public CloudyCreditCardPayment()
        {
            Properties = new Hashtable();
            BillingAddress = new CloudyOrderAddress();
        }

        [JsonConstructor]
        public CloudyCreditCardPayment(CloudyOrderAddress billingAddress)
        {
            BillingAddress = billingAddress;
        }

        public string CardType { get; set; }

        public string CreditCardNumber { get; set; }

        public string CreditCardSecurityCode { get; set; }

        public int ExpirationMonth { get; set; }

        public int ExpirationYear { get; set; }

        public string ProviderPaymentId { get; set; }
    }
}
