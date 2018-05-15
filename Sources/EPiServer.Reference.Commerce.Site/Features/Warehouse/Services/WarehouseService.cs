using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Warehouse.Models;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.InventoryService;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Warehouse.Services
{
    [ServiceConfiguration(typeof(IWarehouseService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class WarehouseService : IWarehouseService
    {
        private readonly IInventoryService _inventoryService;
        private readonly IWarehouseRepository _warehouseRepository;

        public WarehouseService(
            IInventoryService inventoryService,
            IWarehouseRepository warehouseRepository)
        {
            _inventoryService = inventoryService;
            _warehouseRepository = warehouseRepository;
        }

        public IWarehouse GetWarehouse(string warehouseCode)
        {
            return _warehouseRepository.Get(warehouseCode);
        }

        public IEnumerable<WarehouseAvailability> GetWarehouseAvailability(string entryCode)
        {
            var inventory = _inventoryService.QueryByEntry(new[] { entryCode });
            return inventory.Select(x => new WarehouseAvailability
            {
                Warehouse = _warehouseRepository.Get(x.WarehouseCode),
                InStock = x.PurchaseAvailableQuantity
            });
        }
    }
}