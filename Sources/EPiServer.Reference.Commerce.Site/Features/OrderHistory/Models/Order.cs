using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using Mediachase.Commerce.Orders;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.OrderHistory.Models
{
    public class Order
    {
        public PurchaseOrder PurchaseOrder { get; set; }
        public IEnumerable<OrderHistoryItem> Items { get; set; }
        public Address BillingAddress { get; set; }
        public IList<Address> ShippingAddresses { get; set; }
    }
}