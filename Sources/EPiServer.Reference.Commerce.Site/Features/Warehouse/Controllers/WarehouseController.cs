using EPiServer.Reference.Commerce.Site.Features.Warehouse.ViewModelFactories;
using System;
using System.Linq;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Warehouse.Controllers
{
    public class WarehouseController : Controller
    {
        private readonly WarehouseViewModelFactory _warehouseViewModelFactory;

        public WarehouseController(WarehouseViewModelFactory warehouseViewModelFactory)
        {
            _warehouseViewModelFactory = warehouseViewModelFactory;
        }

        [HttpGet]
        public ActionResult Index(string warehouseCode)
        {
            var viewModel = _warehouseViewModelFactory.CreateWarehouseViewModel(warehouseCode);

            if (viewModel.Warehouse == null)
            {
                throw new ArgumentException($"No warehouse having code {warehouseCode} exists.", nameof(warehouseCode));
            }

            return PartialView("_Quickview", viewModel);
        }

        [HttpGet]
        public ActionResult GetAvailability(string skuCode)
        {
            var viewModels = _warehouseViewModelFactory.CreateAvailabilityViewModels(skuCode);

            if (!viewModels.Any())
            {
                return new EmptyResult();
            }

            return PartialView("_Availability", viewModels);
        }
    }
}