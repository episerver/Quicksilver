using Mediachase.Commerce.Inventory;

namespace EPiServer.Reference.Commerce.Site.Features.Warehouse.Models
{
    public class WarehouseAvailability
    {
        public IWarehouse Warehouse { get; set; }
        public decimal InStock { get; set; }
    }
}