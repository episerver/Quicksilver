using EPiServer.Commerce.Order;
using System;

namespace EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes
{
    public class FakeOrderGroupFactory : IOrderGroupFactory
    {
        public IOrderGroupBuilder BuilderFor(IOrderGroup orderGroup)
        {
            throw new NotImplementedException();
        }

        public ICreditCardPayment CreateCardPayment(IOrderGroup orderGroup)
        {
            throw new NotImplementedException();
        }

        public ILineItem CreateLineItem(string code, IOrderGroup orderGroup)
        {
            throw new NotImplementedException();
        }

        public IOrderAddress CreateOrderAddress(IOrderGroup orderGroup)
        {
            throw new NotImplementedException();
        }

        public IOrderForm CreateOrderForm(IOrderGroup orderGroup)
        {
            throw new NotImplementedException();
        }

        public IOrderNote CreateOrderNote(IOrderGroup orderGroup)
        {
            throw new NotImplementedException();
        }

        public IPayment CreatePayment(IOrderGroup orderGroup)
        {
            throw new NotImplementedException();
        }

        public IPayment CreatePayment(IOrderGroup orderGroup, Type paymentType)
        {
            throw new NotImplementedException();
        }

        public IShipment CreateShipment(IOrderGroup orderGroup)
        {
            throw new NotImplementedException();
        }

        public ITaxValue CreateTaxValue(IOrderGroup orderGroup)
        {
            throw new NotImplementedException();
        }
    }
}
