using Newtonsoft.Json;
using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace EPiServer.Commerce.OrderStore.Model
{
    public class PaymentModel
    {
        public decimal Amount { get; set; }

        public string AuthorizationCode { get; set; }

        public OrderAddressModel BillingAddress { get; set; }

        public string CustomerName { get; set; }

        public Guid PaymentMethodId { get; set; }

        public string PaymentMethodName { get; set; }

        [EnumDataType(typeof(PaymentTypeModel))]
        public PaymentTypeModel PaymentType { get; set; }

        public string ProviderTransactionID { get; set; }

        public string ImplementationClass { get; set; }

        public string Status { get; set; }

        public string TransactionID { get; set; }

        public string TransactionType { get; set; }

        public string ValidationCode { get; set; }

        [JsonConverter(typeof(HashTableJsonConverter))]
        public Hashtable Properties { get; set; }

        public string CardType { get; set; }

        public string CreditCardNumber { get; set; }

        public string CreditCardSecurityCode { get; set; }

        public int ExpirationMonth { get; set; }

        public int ExpirationYear { get; set; }

        public string ProviderPaymentId { get; set; }

    }
}