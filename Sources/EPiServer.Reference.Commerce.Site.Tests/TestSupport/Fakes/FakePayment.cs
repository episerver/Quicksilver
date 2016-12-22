using EPiServer.Commerce.Order;
using Mediachase.Commerce.Orders;
using System;
using System.Collections;

namespace EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes
{
    public class FakePayment : IPayment
    {
        private static int _counter;

        public FakePayment()
        {
            PaymentId = ++_counter;
            Properties = new Hashtable();
        }

        public decimal Amount { get; set; }

        public string AuthorizationCode { get; set; }

        public IOrderAddress BillingAddress { get; set; }

        public string CustomerName { get; set; }

        public string ImplementationClass { get; set; }

        public int PaymentId { get; set; }

        public Guid PaymentMethodId { get; set; }

        public string PaymentMethodName { get; set; }

        public PaymentType PaymentType { get; set; }

        public Hashtable Properties { get; private set; }

        public string ProviderTransactionID { get; set; }

        public string Status { get; set; }

        public string TransactionID { get; set; }

        public string TransactionType { get; set; }

        public string ValidationCode { get; set; }
    }
}
