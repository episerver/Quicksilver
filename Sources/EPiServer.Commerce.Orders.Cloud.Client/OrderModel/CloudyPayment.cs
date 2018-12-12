using EPiServer.Commerce.Order;
using EPiServer.Commerce.Serialization.Json;
using Mediachase.Commerce.Orders;
using Newtonsoft.Json;
using System;
using System.Collections;

namespace EPiServer.Commerce.Orders.Cloud.Client.OrderModel
{
    [Serializable]
    [JsonConverter(typeof(PaymentConverter))]
    public class CloudyPayment : IPayment
    {
        [JsonConverter(typeof(HashTableJsonConverter))]
        public Hashtable Properties { get; set; }

        public decimal Amount { get; set; }

        public string AuthorizationCode { get; set; }

        public IOrderAddress BillingAddress { get; set; }

        public string CustomerName { get; set; }

        public string ImplementationClass { get; set; }

        public int PaymentId { get; set; }

        public Guid PaymentMethodId { get; set; }

        public string PaymentMethodName { get; set; }

        public PaymentType PaymentType { get; set; }

        public string ProviderTransactionID { get; set; }

        public string Status { get; set; }

        public string TransactionID { get; set; }

        public string TransactionType { get; set; }

        public string ValidationCode { get; set; }

        [JsonConstructor]
        public CloudyPayment(CloudyOrderAddress billingAddress)
        {
            BillingAddress = billingAddress;
            Properties = new Hashtable();
        }

        public CloudyPayment()
            : this(new CloudyOrderAddress())
        {
        }
    }
}