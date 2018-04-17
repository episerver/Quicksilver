using EPiServer.Reference.Commerce.Site.Features.Warehouse.Models;
using EPiServer.Reference.Commerce.Site.Features.Warehouse.Services;
using EPiServer.Reference.Commerce.Site.Features.Warehouse.ViewModelFactories;
using Moq;
using System.Linq;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Warehouse.ViewModelFactories
{
    public class WarehouseViewModelFactoryTests
    {
        [Fact]
        public void CreateWarehouseViewModel_WhenWarehouseExist_ShouldReturnViewModelContainingWarehouse()
        {
            var viewModel = _subject.CreateWarehouseViewModel(ExistingWarehouseCode);

            Assert.Equal(ExistingWarehouseCode, viewModel.Warehouse.Code);
        }

        [Fact]
        public void CreateAvailabilityViewModels_WhenNoWarehouseExist_ShouldReturnEmptyViewModelCollection()
        {
            var viewModels = _subject.CreateAvailabilityViewModels("nothing-here");

            Assert.Empty(viewModels);
        }

        [Fact]
        public void CreateAvailabilityViewModels_WhenMultipleWarehouses_ShouldReturnViewModelForEach()
        {
            _warehouseServiceMock
                .Setup(x => x.GetWarehouseAvailability(ExistingSkuCode))
                .Returns(new[]
                {
                    new WarehouseAvailability { Warehouse = new Mediachase.Commerce.Inventory.Warehouse() },
                    new WarehouseAvailability { Warehouse = new Mediachase.Commerce.Inventory.Warehouse() }
                });

            var viewModels = _subject.CreateAvailabilityViewModels(ExistingSkuCode);

            Assert.Equal(2, viewModels.Count());
        }

        private readonly WarehouseViewModelFactory _subject;
        private readonly Mock<IWarehouseService> _warehouseServiceMock;
        private const string ExistingSkuCode = "ABC";
        private const string ExistingWarehouseCode = "default";

        public WarehouseViewModelFactoryTests()
        {
            _warehouseServiceMock = new Mock<IWarehouseService>();
            _subject = new WarehouseViewModelFactory(_warehouseServiceMock.Object);

            _warehouseServiceMock
             .Setup(x => x.GetWarehouse(ExistingWarehouseCode))
             .Returns((new Mediachase.Commerce.Inventory.Warehouse { Code = ExistingWarehouseCode, Name = "Default warehouse" }));
        }
    }
}