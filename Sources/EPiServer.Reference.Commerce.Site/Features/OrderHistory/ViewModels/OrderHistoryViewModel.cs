using System.Collections.Generic;
using EPiServer.Reference.Commerce.Site.Features.OrderHistory.Pages;
using EPiServer.Reference.Commerce.Site.Features.Shared.ViewModels;

namespace EPiServer.Reference.Commerce.Site.Features.OrderHistory.ViewModels
{
    public class OrderHistoryViewModel : PageViewModel<OrderHistoryPage>
    {
        public List<OrderViewModel> Orders { get; set; }
    }
}