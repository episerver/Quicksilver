using EPiServer.Reference.Commerce.Site.Features.OrderHistory.Pages;
using EPiServer.Reference.Commerce.Site.Features.Shared.ViewModels;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.OrderHistory.Models
{
    public class OrderHistoryViewModel : PageViewModel<OrderHistoryPage>
    {
        public List<Order> Orders { get; set; }
    }
}