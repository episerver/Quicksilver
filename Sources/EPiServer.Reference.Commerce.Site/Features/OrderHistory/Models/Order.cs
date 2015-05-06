using System.Collections.Generic;
using EPiServer.Reference.Commerce.Site.Features.OrderHistory.Controllers;
using Mediachase.Commerce.Orders;

namespace EPiServer.Reference.Commerce.Site.Features.OrderHistory.Models
{
    public class Order
    {
        public PurchaseOrder PurchaseOrder { get; set; }
        public IEnumerable<OrderHistoryItem> Items { get; set; }
    }
}