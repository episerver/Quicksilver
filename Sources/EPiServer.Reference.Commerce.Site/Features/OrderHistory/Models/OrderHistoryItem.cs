using EPiServer.Commerce.Catalog.ContentTypes;
using Mediachase.Commerce.Orders;

namespace EPiServer.Reference.Commerce.Site.Features.OrderHistory.Models
{
    public class OrderHistoryItem
    {
        public LineItem LineItem { get; set; }
        public VariationContent Variation { get; set; }
    }
}