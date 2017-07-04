using EPiServer.Reference.Commerce.Site.Features.Warehouse.Services;
using EPiServer.Reference.Commerce.Site.Features.Warehouse.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Warehouse.ViewModelFactories
{
    public class WarehouseViewModelFactory
    {
        private readonly IWarehouseService _warehouseService;

        public WarehouseViewModelFactory(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        public virtual IEnumerable<WarehouseInventoryViewModel> CreateAvailabilityViewModels(string skuCode)
        {
            return _warehouseService
                .GetWarehouseAvailability(skuCode)
                .Select(x => new WarehouseInventoryViewModel
                {
                    WarehouseName = x.Warehouse.Name,
                    InStockQuantity = x.InStock,
                    WarehouseCode = x.Warehouse.Code,
                    SkuCode = skuCode
                });
        }

        public virtual WarehouseViewModel CreateWarehouseViewModel(string warehouseCode)
        {
            var viewModel = new WarehouseViewModel
            {
                Warehouse = _warehouseService.GetWarehouse(warehouseCode)
            };

            return viewModel;
        }
    }
}