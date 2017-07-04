namespace EPiServer.Reference.Commerce.Site.Features.Warehouse.ViewModels
{
    public class WarehouseInventoryViewModel
    {
        public string WarehouseName { get; set; }
        public string WarehouseCode { get; set; }
        public string SkuCode { get; set; }
        public decimal InStockQuantity { get; set; }
    }
}