using EPiServer.Commerce.Order;
using System;
using System.Collections.Generic;

namespace EPiServer.Commerce.Orders.Cloud.Client
{
    public class PaymentPlanClient<T>  where T : IPaymentPlan
    {
      
        public T Load(int orderGroupId)
        {
            throw new NotImplementedException();

        }

        public IEnumerable<T> Load(Guid customerId, string name)
        {
            throw new NotImplementedException();

        }

        public T Save(IPaymentPlan paymentPlan)
        {
            throw new NotImplementedException();

        }

        public void Delete(int orderGroupId)
        {
            throw new NotImplementedException();

        }

    }
}
