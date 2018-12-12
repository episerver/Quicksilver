using EPiServer.Commerce.Order;

namespace EPiServer.Commerce.Orders.Cloud.Client.OrderModel
{
    public class CloudyReturnLineItem : CloudyLineItem, IReturnLineItem
    {
        public int? OriginalLineItemId { get; set; }
        public string ReturnReason { get; set; }
    }
}
