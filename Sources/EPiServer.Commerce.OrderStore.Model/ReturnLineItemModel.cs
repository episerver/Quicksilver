namespace EPiServer.Commerce.OrderStore.Model
{
    public class ReturnLineItemModel : LineItemModel
    {
        public int? OriginalLineItemId { get; set; }
        public string ReturnReason { get; set; }
    }
}
