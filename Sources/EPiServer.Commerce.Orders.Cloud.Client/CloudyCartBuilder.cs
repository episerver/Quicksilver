using EPiServer.Commerce.Order;
using EPiServer.Commerce.Orders.Cloud.Client.OrderModel;
using Mediachase.Commerce.Orders;
using System;

namespace EPiServer.Commerce.Orders.Cloud.Client
{
    public class CloudyCartBuilder : IOrderGroupBuilder
    {
        public int SortOrder => 0;

        public Type[] ForType => new[] { typeof(CloudyCart), typeof(CloudyOrder), typeof(CloudyPaymentPlan) };

        public IOrderForm CreateOrderForm()
        {
            return new CloudyOrderForm();
        }

        public IShipment CreateShipment()
        {
            return new CloudyShipment();
        }

        public ILineItem CreateLineItem(string code)
        {
            return new CloudyLineItem() { Code = code };
        }

        public IOrderAddress CreateOrderAddress()
        {
            return new CloudyOrderAddress();
        }

        public IOrderNote CreateOrderNote()
        {
            return new CloudyOrderNote();
        }

        public IPayment CreatePayment()
        {
            return new CloudyPayment { ImplementationClass = typeof(CloudyPayment).AssemblyQualifiedName };
        }

        public IPayment CreatePayment(Type paymentType)
        {
            return new CloudyPayment { ImplementationClass = typeof(CloudyPayment).AssemblyQualifiedName };
        }

        public ICreditCardPayment CreateCardPayment()
        {
            return new CloudyCreditCardPayment { ImplementationClass = typeof(CloudyCreditCardPayment).AssemblyQualifiedName };
        }

        public ITaxValue CreateTaxValue()
        {
            return new TaxValue();
        }
    }
}
