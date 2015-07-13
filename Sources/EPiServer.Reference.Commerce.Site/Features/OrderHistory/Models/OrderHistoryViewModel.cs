using System.Collections.Generic;
using EPiServer.Reference.Commerce.Site.Features.OrderHistory.Pages;

namespace EPiServer.Reference.Commerce.Site.Features.OrderHistory.Models
{
    public class OrderHistoryViewModel
    {
        public OrderHistoryPage CurrentPage { get; set; }
        public List<Order> Orders { get; set; }
    }
}