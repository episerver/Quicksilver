using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Warehouse.Models;
using EPiServer.Reference.Commerce.Site.Features.Warehouse.Services;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.InventoryService;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Warehouse.Services
{
    public class WarehouseServiceTests
    {
        [Fact]
        public void GetWarehouse_WhenExists_ShouldReturnWarehouseWithAddress()
        {
            var warehouse = _subject.GetWarehouse(ExistingWarehouseCode);

            Assert.NotNull(warehouse.ContactInformation);
        }

        [Fact]
        public void GetWarehouse_WhenDoNotExist_ShouldReturnNull()
        {
            var warehouse = _subject.GetWarehouse("no-place-like-this");

            Assert.Null(warehouse);
        }

        [Fact]
        public void GetWarehouseInventories_WhenProductAndWarehouseExist_ShouldReturnInventoryWithWarehouse()
        {
            _inventoryServiceMock
                .Setup(x => x.QueryByEntry(It.IsAny<IEnumerable<string>>()))
                .Returns(new List<InventoryRecord>(new[]
                {
                    new InventoryRecord
                    {
                        WarehouseCode = ExistingWarehouseCode
                    }
                }));

            var availability = _subject.GetWarehouseAvailability("existing-product");

            Assert.NotNull(availability.Single().Warehouse);
        }

        [Fact]
        public void GetWarehouseInventories_WhenProductDoesNotExist_ShouldReturnEmptyCollection()
        {
            _inventoryServiceMock
                .Setup(x => x.QueryByEntry(It.IsAny<IEnumerable<string>>()))
                .Returns(new List<InventoryRecord>());

            var inventories = _subject.GetWarehouseAvailability("no-such-product");

            Assert.Empty(inventories);
        }

        private readonly Mock<IInventoryService> _inventoryServiceMock;
        private readonly Mock<IWarehouseRepository> _warehouseRepositoryMock;
        private readonly IWarehouseService _subject;
        private const string ExistingWarehouseCode = "default";

        public WarehouseServiceTests()
        {
            _inventoryServiceMock = new Mock<IInventoryService>();
            _warehouseRepositoryMock = new Mock<IWarehouseRepository>();

            _subject = new WarehouseService(
                _inventoryServiceMock.Object,
                _warehouseRepositoryMock.Object);

            _warehouseRepositoryMock
                .Setup(x => x.Get(ExistingWarehouseCode))
                .Returns(new Mediachase.Commerce.Inventory.Warehouse
                {
                    Code = ExistingWarehouseCode,
                    Name = "Default warehouse",
                    ContactInformation = new WarehouseContactInformation()
                });
        }
    }
}