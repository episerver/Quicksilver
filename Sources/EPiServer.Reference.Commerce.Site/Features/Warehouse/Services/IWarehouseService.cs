using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Warehouse.Models;
using Mediachase.Commerce.Inventory;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Warehouse.Services
{
    public interface IWarehouseService
    {
        IWarehouse GetWarehouse(string warehouseCode);
        IEnumerable<WarehouseAvailability> GetWarehouseAvailability(string entryCode);
    }
}