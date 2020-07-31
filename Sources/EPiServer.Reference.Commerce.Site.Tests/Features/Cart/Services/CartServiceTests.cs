using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Internal;
using EPiServer.Core;
using EPiServer.Framework.Cache;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Product.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Mediachase.Commerce.Pricing;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Cart.Services
{
    public class CartServiceTests
    {
        [Fact]
        public void AddCouponCode_WhenCouponCodeIsAppliedToPromotion_ShouldReturnTrue()
        {
            var couponCode = "IBUYALOT";

            var rewardDescription = RewardDescription.CreatePercentageReward(FulfillmentStatus.Fulfilled, null, new PromotionForTest(), 10, string.Empty);
            rewardDescription.AppliedCoupon = couponCode;
            _promotionEngineMock
                .Setup(x => x.Run(It.IsAny<IOrderGroup>(), It.IsAny<PromotionEngineSettings>()))
                .Returns(new[] { rewardDescription });

            var result = _subject.AddCouponCode(_cart, couponCode);

            Assert.True(result);
        }

        [Fact]
        public void AddCouponCode_WhenCouponCodeIsNotAppliedToPromotion_ShouldReturnFalse()
        {
            var couponCode = "IDONTEXIST";

            _promotionEngineMock
                .Setup(x => x.Run(It.IsAny<IOrderGroup>(), It.IsAny<PromotionEngineSettings>()))
                .Returns(new[] {
                    RewardDescription.CreatePercentageReward(FulfillmentStatus.Fulfilled, null, new PromotionForTest(), 10, string.Empty)
                });

            var result = _subject.AddCouponCode(_cart, couponCode);

            Assert.False(result);
        }

        [Fact]
        public void RemoveCouponCode_ShouldRemoveCodeFromForm()
        {
            string couponCode = "IBUYALOT";
            _cart.GetFirstForm().CouponCodes.Add(couponCode);
            _subject.RemoveCouponCode(_cart, couponCode);

            Assert.Empty(_cart.GetFirstForm().CouponCodes);
        }

        [Fact]
        public void AddToCart_ShouldRunPromotionEngine()
        {
            var code = "EAN";
            _subject.AddToCart(_cart, code, 1);

            _orderValidationServiceMock.Verify(x => x.ValidateOrder(_cart), Times.Once);
        }

        [Fact]
        public void AddToCart_WhenLineItemNotInCart_ShouldAddToCart()
        {
            _subject.AddToCart(_cart, "code", 1);

            Assert.Equal(1, _cart.GetAllLineItems().Single(x => x.Code == "code").Quantity);
        }

        [Fact]
        public void AddToCart_WhenLineItemNotInCart_ShouldAddLineItemDisplayName()
        {
            _variationContent.DisplayName = "sample-name";

            _subject.AddToCart(_cart, "code", 1);

            Assert.Equal("sample-name", _cart.GetAllLineItems().Single(x => x.Code == "code").DisplayName);
        }

        [Fact]
        public void AddToCart_WhenLineItemNotInCart_ShouldSetPlacedPrice()
        {
            var code = "code";
            var unitPrice = new Money(9.95m, _cart.Currency);

            _pricingServiceMock
                .Setup(m => m.GetPrice(code))
                .Returns(new PriceValue {UnitPrice = unitPrice});
            _subject.AddToCart(_cart, code, 1);

            Assert.Equal(unitPrice.Amount, _cart.GetAllLineItems().Single(x => x.Code == code).PlacedPrice);
        }

        [Fact]
        public void AddToCart_WhenLineItemAlreadyInCart_ShouldIncreaseQuantity()
        {
            _subject.AddToCart(_cart, "code", 1);
            _subject.AddToCart(_cart, "code", 1);

            Assert.Equal(2, _cart.GetAllLineItems().Single(x => x.Code == "code").Quantity);
        }

        [Fact]
        public void AddToCart_WhenLineItemIsRemoved_ShouldReturnWarningMessage()
        {
            var validationIssues = new Dictionary<ILineItem, IList<ValidationIssue>>
            {
                {
                    new Mock<ILineItem>().Object, new List<ValidationIssue>()
                    {
                        ValidationIssue.AdjustedQuantityByAvailableQuantity
                    }
                }
            };
            _orderValidationServiceMock
                .Setup(x => x.ValidateOrder(_cart))
                .Returns(validationIssues);

            var result = _subject.AddToCart(_cart, "SKU1234", 1);
            Assert.Equal<int>(1, result.ValidationMessages.Count);
        }

        [Fact]
        public void AddToCart_WhenItemIsBundle_ShouldAddContainedLineItemsToCart()
        {
            var bundle = new BundleContent
            {
                ContentLink = new ContentReference(1),
                Code = "bundlecode"
            };

            var bundleEntry1 = new BundleEntry
            {
                Parent = new ContentReference(2),
                Quantity = 1
            };

            var variant1 = new VariationContent
            {
                Code = "variant1code",
                ContentLink = bundleEntry1.Parent
            };

            var bundleEntry2 = new BundleEntry
            {
                Parent = new ContentReference(3),
                Quantity = 2
            };

            var variant2 = new VariationContent
            {
                Code = "variant2code",
                ContentLink = bundleEntry2.Parent
            };

            _referenceConverterMock.Setup(x => x.GetContentLink(variant1.Code)).Returns(variant1.ContentLink);
            _referenceConverterMock.Setup(x => x.GetContentLink(variant2.Code)).Returns(variant2.ContentLink);
            _referenceConverterMock.Setup(x => x.GetContentLink(bundle.Code)).Returns(bundle.ContentLink);

            _relationRepositoryMock.Setup(x => x.GetChildren<BundleEntry>(bundle.ContentLink))
                .Returns(() => new List<BundleEntry> { bundleEntry1, bundleEntry2 });

            _contentLoaderMock.Setup(x => x.Get<EntryContentBase>(bundle.ContentLink)).Returns(bundle);
            _contentLoaderMock.Setup(x => x.Get<EntryContentBase>(variant1.ContentLink)).Returns(variant1);
            _contentLoaderMock.Setup(x => x.Get<EntryContentBase>(variant2.ContentLink)).Returns(variant2);

            _subject.AddToCart(_cart, bundle.Code, 1);

            Assert.Equal(bundleEntry1.Quantity + bundleEntry2.Quantity, _cart.GetAllLineItems().Sum(x => x.Quantity));
        }

        [Fact]
        public void AddToCart_WhenItemIsBundleAndContainsPackage_ShouldAddContainedPackageToCart()
        {
            var bundle = new BundleContent
            {
                ContentLink = new ContentReference(1),
                Code = "bundlecode"
            };

            var bundleEntry1 = new BundleEntry
            {
                Parent = new ContentReference(2),
                Quantity = 1
            };

            var variant1 = new VariationContent
            {
                Code = "variant1code",
                ContentLink = bundleEntry1.Parent
            };

            var bundleEntry2 = new BundleEntry
            {
                Parent = new ContentReference(3),
                Quantity = 2
            };

            var package = new PackageContent
            {
                Code = "packagecode",
                ContentLink = bundleEntry2.Parent
            };

            _referenceConverterMock.Setup(x => x.GetContentLink(variant1.Code)).Returns(variant1.ContentLink);
            _referenceConverterMock.Setup(x => x.GetContentLink(package.Code)).Returns(package.ContentLink);
            _referenceConverterMock.Setup(x => x.GetContentLink(bundle.Code)).Returns(bundle.ContentLink);

            _relationRepositoryMock.Setup(x => x.GetChildren<BundleEntry>(bundle.ContentLink))
                .Returns(() => new List<BundleEntry> { bundleEntry1, bundleEntry2 });

            _contentLoaderMock.Setup(x => x.Get<EntryContentBase>(bundle.ContentLink)).Returns(bundle);
            _contentLoaderMock.Setup(x => x.Get<EntryContentBase>(variant1.ContentLink)).Returns(variant1);
            _contentLoaderMock.Setup(x => x.Get<EntryContentBase>(package.ContentLink)).Returns(package);

            _subject.AddToCart(_cart, bundle.Code, 1);

            Assert.Equal(bundleEntry1.Quantity + bundleEntry2.Quantity, _cart.GetAllLineItems().Sum(x => x.Quantity));
        }

        [Fact]
        public void ChangeCartItem_ShouldChangeQuantityAccordingly()
        {
            var shipmentId = 0;
            var quantity = 5m;
            var size = "small";
            var code = "EAN";
            var newSize = "small";
            var lineItem = new InMemoryLineItem
            {
                Quantity = 2,
                Code = code
            };

            _cart.GetFirstShipment().LineItems.Add(lineItem);

            _subject.ChangeCartItem(_cart, shipmentId, code, quantity, size, newSize, "");

            Assert.Equal<decimal>(quantity, lineItem.Quantity);
        }

        [Fact]
        public void ChangeCartItem_WhenQuantityIsZero_ShouldRemoveLineItem()
        {
            var shipmentId = 0;
            var quantity = 0;
            var size = "small";
            var code = "EAN";
            var newSize = "small";
            var lineItem = new InMemoryLineItem
            {
                Quantity = 2,
                Code = code
            };

            _cart.GetFirstShipment().LineItems.Add(lineItem);

            _subject.ChangeCartItem(_cart, shipmentId, code, quantity, size, newSize, "");

            Assert.Empty(_cart.GetAllLineItems());
        }

        [Fact]
        public void ChangeCartItemForMultiShipment_WhenQuantityIsZeroInOneShipment_ShouldRemoveTheCorrespondantLineItem()
        {
            var quantity = 0;
            var size = "small";
            var code = "EAN";
            var newSize = "small";
            var lineItem = new InMemoryLineItem
            {
                Quantity = 2,
                Code = code
            };

            var removedLineItem = new InMemoryLineItem
            {
                Quantity = 3,
                Code = code
            };

            _cart.GetFirstShipment().LineItems.Add(lineItem);
            var shipment = new FakeShipment { ParentOrderGroup = _cart};
            shipment.LineItems.Add(removedLineItem);
            _cart.AddShipment(shipment, _orderGroupFactoryMock.Object);
            _subject.ChangeCartItem(_cart, shipment.ShipmentId, code, quantity, size, newSize, "");

           Assert.DoesNotContain(_cart.GetFirstForm().Shipments, s => s.ShipmentId == shipment.ShipmentId && s.LineItems.Any(l => l.Code == code));
        }

        [Fact]
        public void ChangeCartItem_WhenLineItemToChangeToDoesNotExists_ShouldUpdateLineItem()
        {
            _productServiceMock.Setup(x => x.GetSiblingVariantCodeBySize(It.IsAny<string>(), It.IsAny<string>())).Returns("newcode");

            _cart.GetFirstShipment().LineItems.Add(new InMemoryLineItem { Quantity = 1, Code = "code" });

            _subject.ChangeCartItem(_cart, 0, "code", 5, "S", "M", "");

            Assert.Equal("newcode", _cart.GetFirstShipment().LineItems.Single().Code);
            Assert.Equal<decimal>(5, _cart.GetFirstShipment().LineItems.Single().Quantity);
        }

        [Fact]
        public void ChangeCartItem_WhenLineItemToChangeToAlreadyExists_ShouldUpdateLineItem()
        {
            _productServiceMock.Setup(x => x.GetSiblingVariantCodeBySize(It.IsAny<string>(), It.IsAny<string>())).Returns("newcode");

            _cart.GetFirstShipment().LineItems.Add(new InMemoryLineItem { Quantity = 1, Code = "newcode" });
            _cart.GetFirstShipment().LineItems.Add(new InMemoryLineItem { Quantity = 1, Code = "code" });

            _subject.ChangeCartItem(_cart, 0, "code", 5, "S", "M", "");

            Assert.Equal("newcode", _cart.GetFirstShipment().LineItems.Single().Code);
            Assert.Equal<decimal>(5 + 1, _cart.GetFirstShipment().LineItems.Single().Quantity);
        }

        [Fact]
        public void LoadCart_WhenNotExist_ShouldReturnNUll()
        {
            var result = _subject.LoadCart("UNKNOWN");

            Assert.Null(result);
        }

        [Fact]
        public void LoadCart_WhenExist_ShouldReturnInstanceOfCart()
        {
            var result = _subject.LoadCart(_subject.DefaultCartName);

            Assert.Equal<ICart>(_cart, result);
            Assert.Equal(result.Currency, new Currency("USD"));
        }

        [Fact]
        public void LoadCart_WithCurrentCurrency_WhenExist_ShouldReturnInstanceOfCart()
        {
            var currentCurrency = new Currency("SEK");
            _currencyServiceMock.Setup(x => x.GetCurrentCurrency()).Returns(currentCurrency);

            var result = _subject.LoadCart(_subject.DefaultCartName);

            Assert.Equal(result.Currency, currentCurrency);
        }

        [Fact]
        public void LoadCart_WhenAllItemsIsValid_ShouldReturnCartWithAllItems()
        {
            var lineItem = new InMemoryLineItem { Quantity = 2, Code = "code1" };
            var lineItem2 = new InMemoryLineItem { Quantity = 2, Code = "code2" };
            _cart.GetFirstShipment().LineItems.Add(lineItem);
            _cart.GetFirstShipment().LineItems.Add(lineItem2);

            var result = _subject.LoadCart(_subject.DefaultCartName);

            Assert.Equal(2, result.GetAllLineItems().Count());
        }

        [Fact]
        public void LoadOrCreateCart_WhenExist_ShouldReturnInstanceOfCart()
        {
            var result = _subject.LoadOrCreateCart(_subject.DefaultCartName);

            Assert.Equal<ICart>(_cart, result);
            Assert.Equal(result.Currency, new Currency("USD"));
        }

        [Fact]
        public void LoadOrCreateCart_WithCurrentCurrency_WhenExist_ShouldReturnInstanceOfCart()
        {
            var currentCurrency = new Currency("SEK");
            _currencyServiceMock.Setup(x => x.GetCurrentCurrency()).Returns(currentCurrency);

            var result = _subject.LoadOrCreateCart(_subject.DefaultCartName);

            Assert.Equal(result.Currency, currentCurrency);
        }

        [Fact]
        public void LoadOrCreateCart_WhenNoCartExists_ShouldReturnNewInstanceOfCart()
        {
            _orderRepositoryMock
                .Setup(x => x.Create<ICart>(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(new FakeCart(_marketMock.Object, new Currency("USD")));

            var result = _subject.LoadOrCreateCart("UNKNOWN");

            Assert.NotNull(result);
            Assert.NotEqual<ICart>(_cart, result);
        }

        [Fact]
        public void MergeShipments()
        {
            _subject.MergeShipments(_cart);
        }

        [Fact]
        public void RecreateLineItemsBasedOnShipments_WhenHavingLineItemDisplayName_ShouldSaveToCart()
        {
            var shipment = _cart.GetFirstShipment();
            var skuCode = "EAN";
            var displayName = "display-name";

            shipment.LineItems.Add(new InMemoryLineItem
            {
                Code = skuCode,
                Quantity = 2
            });

            _subject.RecreateLineItemsBasedOnShipments(_cart, new[]
            {
                new CartItemViewModel { Code = skuCode, AddressId = "1", DisplayName = displayName },
                new CartItemViewModel { Code = skuCode, AddressId = "2", DisplayName = displayName }
            }, new[]
            {
                new AddressModel { AddressId = "1", Line1 = "First street" },
                new AddressModel { AddressId = "2", Line1 = "Second street" }
            });

            Assert.Equal(displayName, _cart.GetAllLineItems().First().DisplayName);
        }

        [Fact]
        public void RecreateLineItemsBasedOnShipments_WhenHavingTwoShippingAddresses_ShouldCreateTwoShipmentLineItems()
        {
            var shipment = _cart.GetFirstShipment();
            var skuCode = "EAN";

            shipment.LineItems.Add(new InMemoryLineItem
            {
                Code = skuCode,
                Quantity = 3
            });

            _subject.RecreateLineItemsBasedOnShipments(_cart, new[]
            {
                new CartItemViewModel { Code = skuCode, AddressId = "1" },
                new CartItemViewModel { Code = skuCode, AddressId = "2" },
                new CartItemViewModel { Code = skuCode, AddressId = "2" }
            }, new[]
            {
                new AddressModel { AddressId = "1", Line1 = "First street" },
                new AddressModel { AddressId = "2", Line1 = "Second street" }
            });

            Assert.Equal<int>(2, _cart.GetAllLineItems().Count());
        }

        [Fact]
        public void RecreateLineItemsBasedOnShipments_WhenHavingLineItemAsGift_ShouldKeepIsGift()
        {
            var shipment = _cart.GetFirstShipment();
            var skuCode = "EAN";
            var giftSkuCode = "AAA";

            shipment.LineItems.Add(new InMemoryLineItem
            {
                Code = giftSkuCode,
                Quantity = 1,
                IsGift = true
            });

            shipment.LineItems.Add(new InMemoryLineItem
            {
                Code = skuCode,
                Quantity = 2,
                IsGift = false
            });

            _subject.RecreateLineItemsBasedOnShipments(_cart, new[]
            {
                new CartItemViewModel { Code = skuCode, AddressId = "1", IsGift = false},
                new CartItemViewModel { Code = skuCode, AddressId = "2", IsGift = false},
                new CartItemViewModel { Code = giftSkuCode, AddressId = "2", IsGift = true},
            }, new[]
            {
                new AddressModel { AddressId = "1", Line1 = "First street" },
                new AddressModel { AddressId = "2", Line1 = "Second street" }
            });

            Assert.Single(_cart.GetAllLineItems().Where(x => x.Code == giftSkuCode && x.IsGift));
            Assert.Equal<int>(2, _cart.GetAllLineItems().Count(x => x.Code == skuCode && !x.IsGift));
        }

        [Fact]
        public void RecreateLineItemsBasedOnShipments_WhenHavingLineItemWithSameCodeAsGiftAndNotGift_ShouldKeepIsGift()
        {
            var shipment = _cart.GetFirstShipment();
            var skuCode = "EAN";

            shipment.LineItems.Add(new InMemoryLineItem
            {
                Code = skuCode,
                Quantity = 1,
                IsGift = true
            });

            shipment.LineItems.Add(new InMemoryLineItem
            {
                Code = skuCode,
                Quantity = 2,
                IsGift = false
            });

            _subject.RecreateLineItemsBasedOnShipments(_cart, new[]
            {
                new CartItemViewModel { Code = skuCode, AddressId = "1", IsGift = false},
                new CartItemViewModel { Code = skuCode, AddressId = "2" , IsGift = false},
                new CartItemViewModel { Code = skuCode, AddressId = "2" , IsGift = true}
            }, new[]
            {
                new AddressModel { AddressId = "1", Line1 = "First street" },
                new AddressModel { AddressId = "2", Line1 = "Second street" }
            });
            Assert.Equal<int>(2, _cart.GetAllLineItems().Count(x => x.Code == skuCode && !x.IsGift));
            Assert.Single(_cart.GetAllLineItems().Where(x => x.Code == skuCode && x.IsGift));
        }

        [Fact]
        public void RequestInventory()
        {
            _subject.RequestInventory(_cart);
        }

        [Fact]
        public void ValidateCart()
        {
            var result = _subject.ValidateCart(_cart);
        }

        [Fact]
        public void ValidateCart_WhenIsWishList_ShouldReturnEmptyDictionary()
        {
            _cart.Name = _subject.DefaultWishListName;
            var expectedResult = new Dictionary<ILineItem, IList<ValidationIssue>>();
            var result = _subject.ValidateCart(_cart);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void AddToCart_WhenExistSameItemIsGiftInCart_ShouldAddToCart()
        {
            var shipment = _cart.GetFirstShipment();
            var skuCode = "EAN";

            shipment.LineItems.Add(new InMemoryLineItem
            {
                Code = skuCode,
                Quantity = 1,
                IsGift = true
            });

            _subject.AddToCart(_cart, skuCode, 1);

            Assert.Equal(2, _cart.GetAllLineItems().Count());
        }

        private readonly Mock<IAddressBookService> _addressBookServiceMock;
        private readonly Mock<CustomerContextFacade> _customerContextFacaceMock;
        private readonly Mock<IOrderGroupFactory> _orderGroupFactoryMock;
        private readonly Mock<IInventoryProcessor> _inventoryProcessorMock;
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IPricingService> _pricingServiceMock;
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<IPromotionEngine> _promotionEngineMock;
        private readonly Mock<ICurrentMarket> _currentMarketMock;
        private readonly Mock<IMarket> _marketMock;
        private readonly Mock<ICurrencyService> _currencyServiceMock;
        private readonly Mock<ReferenceConverter> _referenceConverterMock;
        private readonly Mock<IContentLoader> _contentLoaderMock;
        private readonly Mock<IRelationRepository> _relationRepositoryMock;
        private readonly CartService _subject;
        private readonly ICart _cart;
        private readonly VariationContent _variationContent;
        private readonly Mock<OrderValidationService> _orderValidationServiceMock;

        public CartServiceTests()
        {
            var synchronizedObjectInstanceCacheMock = new Mock<ISynchronizedObjectInstanceCache>();
            _addressBookServiceMock = new Mock<IAddressBookService>();
            _customerContextFacaceMock = new Mock<CustomerContextFacade>(null);
            _orderGroupFactoryMock = new Mock<IOrderGroupFactory>();
            _inventoryProcessorMock = new Mock<IInventoryProcessor>();
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _pricingServiceMock = new Mock<IPricingService>();
            _productServiceMock = new Mock<IProductService>();
            _productServiceMock = new Mock<IProductService>();
            _promotionEngineMock = new Mock<IPromotionEngine>();
            _marketMock = new Mock<IMarket>();
            _currentMarketMock = new Mock<ICurrentMarket>();
            _currencyServiceMock = new Mock<ICurrencyService>();
            _contentLoaderMock = new Mock<IContentLoader>();
            _variationContent = new VariationContent();
            _relationRepositoryMock = new Mock<IRelationRepository>();
            _orderValidationServiceMock =
                new Mock<OrderValidationService>(null, null, null, null);
            _referenceConverterMock = new Mock<ReferenceConverter>(
                new EntryIdentityResolver(synchronizedObjectInstanceCacheMock.Object, new CatalogOptions()), 
                new NodeIdentityResolver(synchronizedObjectInstanceCacheMock.Object, new CatalogOptions()));

            _orderValidationServiceMock.Setup(x => x.ValidateOrder(It.IsAny<IOrderGroup>()))
                .Returns(new Dictionary<ILineItem, IList<ValidationIssue>>());

            _subject = new CartService(_productServiceMock.Object, _pricingServiceMock.Object, _orderGroupFactoryMock.Object,
                _customerContextFacaceMock.Object, _inventoryProcessorMock.Object, _orderRepositoryMock.Object, _promotionEngineMock.Object,
                _addressBookServiceMock.Object, _currentMarketMock.Object, _currencyServiceMock.Object,
                _referenceConverterMock.Object, _contentLoaderMock.Object, _relationRepositoryMock.Object,
                _orderValidationServiceMock.Object);
            _cart = new FakeCart(new Mock<IMarket>().Object, new Currency("USD"))
            {
                Name = _subject.DefaultCartName,
                OrderLink = new OrderReference(1, "Default", Guid.NewGuid(), typeof(FakeCart))
            };

            _orderGroupFactoryMock.Setup(x => x.CreateLineItem(It.IsAny<string>(), It.IsAny<IOrderGroup>())).Returns((string code, IOrderGroup orderGroup) => new FakeLineItem { Code = code, ParentOrderGroup = orderGroup });
            _orderGroupFactoryMock.Setup(x => x.CreateShipment(It.IsAny<IOrderGroup>())).Returns((IOrderGroup orderGroup) => new FakeShipment { ParentOrderGroup = orderGroup });
            _orderRepositoryMock.Setup(x => x.Load<ICart>(It.IsAny<Guid>(), _subject.DefaultCartName)).Returns(new[] { _cart });
            _orderRepositoryMock.Setup(x => x.Create<ICart>(It.IsAny<Guid>(), _subject.DefaultCartName)).Returns(_cart);
            _currentMarketMock.Setup(x => x.GetCurrentMarket()).Returns(_marketMock.Object);
            _contentLoaderMock.Setup(x => x.Get<EntryContentBase>(It.IsAny<ContentReference>())).Returns(_variationContent);
        }

        class PromotionForTest : EntryPromotion
        {
        }
    }
}
