using EPiServer.Reference.Commerce.Site.Features.Warehouse.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Warehouse.Services;
using EPiServer.Reference.Commerce.Site.Features.Warehouse.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Warehouse.ViewModels;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Warehouse.Controllers
{
    public class WarehouseControllerTests
    {
        [Fact]
        public void Index_WhenWarehouseExists_ShouldReturnViewModelWithWarehouse()
        {
            var viewModel = ((PartialViewResult)_subject.Index(ExistingWarehouseCode)).Model as WarehouseViewModel;

            Assert.Equal("default", viewModel.Warehouse.Code);
        }

        [Fact]
        public void Index_WhenWarehouseDoNotExist_ShouldThrowException()
        {
            Assert.Throws<ArgumentException>(() => _subject.Index("nobody-home"));
        }

        [Fact]
        public void GetAvailability_WhenSkuExists_ShouldReturnViewModelWithWarehouse()
        {
            var viewModels = ((PartialViewResult)_subject.GetAvailability(ExistingSkuCode)).Model as IEnumerable<WarehouseInventoryViewModel>;

            Assert.Equal("default", viewModels.Single().WarehouseCode);
        }

        [Fact]
        public void GetAvailability_WhenNoStock_ShouldReturnEmptyResult()
        {
            _warehouseViewModelFactoryMock
                .Setup(x => x.CreateAvailabilityViewModels(ExistingSkuCode))
                .Returns(Enumerable.Empty<WarehouseInventoryViewModel>());

            var result = _subject.GetAvailability(ExistingSkuCode);

            Assert.IsType<EmptyResult>(result);
        }

        private const string ExistingWarehouseCode = "default";
        private const string ExistingSkuCode = "ABC";
        private readonly WarehouseController _subject;
        private readonly Mock<WarehouseViewModelFactory> _warehouseViewModelFactoryMock;
        private readonly Mock<IWarehouseService> _warehouseService;

        public WarehouseControllerTests()
        {
            var warehouse = GetExistingWarehouse();
            _warehouseService = new Mock<IWarehouseService>();
            _warehouseViewModelFactoryMock = new Mock<WarehouseViewModelFactory>(_warehouseService.Object);
            _subject = new WarehouseController(_warehouseViewModelFactoryMock.Object);

            _warehouseService
                .Setup(x => x.GetWarehouse(ExistingWarehouseCode))
                .Returns(warehouse);

            _warehouseViewModelFactoryMock
                .Setup(x => x.CreateWarehouseViewModel(It.IsAny<string>()))
                .Returns(new WarehouseViewModel());

            _warehouseViewModelFactoryMock
                .Setup(x => x.CreateWarehouseViewModel(ExistingWarehouseCode))
                .Returns(new WarehouseViewModel
                {
                    Warehouse = warehouse
                });

            _warehouseViewModelFactoryMock
                .Setup(x => x.CreateAvailabilityViewModels(ExistingSkuCode))
                .Returns(new[]
                {
                    new WarehouseInventoryViewModel
                    {
                        SkuCode = ExistingSkuCode,
                        InStockQuantity = 100,
                        WarehouseCode = warehouse.Code,
                        WarehouseName = warehouse.Name
                    }
                });
        }

        private Mediachase.Commerce.Inventory.Warehouse GetExistingWarehouse()
        {
            return new Mediachase.Commerce.Inventory.Warehouse
            {
                Code = ExistingWarehouseCode,
                Name = "Default warehouse",
                ContactInformation = new Mediachase.Commerce.Inventory.WarehouseContactInformation
                {
                    FirstName = "John",
                    LastName = "Doe",
                    City = "Stockholm",
                    CountryCode = "SE",
                    CountryName = "Sweden",
                    Email = "test@example.com"
                }
            };
        }
    }
}