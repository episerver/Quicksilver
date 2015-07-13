using System;
using System.Collections.Specialized;
using System.Linq;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Search;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Services
{
    public class ConfirmationService
    {
        public PurchaseOrder GetOrder(int orderNumber, bool mock = false)
        {
            return !mock ? OrderContext.Current.GetPurchaseOrder(orderNumber) : GetMockOrder();
        }

        private PurchaseOrder GetMockOrder()
        {
            var parameters = new OrderSearchParameters
            {
                SqlMetaWhereClause = "[ObjectId] = (SELECT TOP 1 OrderGroupId FROM OrderGroup order by Total desc)"
            };

            var options = new OrderSearchOptions
            {
                RecordsToRetrieve = 1,
                Classes = new StringCollection { "PurchaseOrder" }
            };

            return OrderContext.Current.FindPurchaseOrders(parameters, options).FirstOrDefault();
        }
    }
}