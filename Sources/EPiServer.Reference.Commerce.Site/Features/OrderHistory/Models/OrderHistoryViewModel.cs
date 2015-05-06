using System.Collections.Generic;
using EPiServer.Reference.Commerce.Site.Features.OrderHistory.Page;
using Mediachase.Commerce.Orders;

namespace EPiServer.Reference.Commerce.Site.Features.OrderHistory.Models
{
    public class OrderHistoryViewModel
    {
        public OrderHistoryPage CurrentPage { get; set; }
        public List<Order> Orders { get; set; }
    }
}