using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Internal;
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
using Mediachase.Commerce.Customers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
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
            string warningMessage = null;
            _subject.AddToCart(_cart, code, out warningMessage);

            _promotionEngineMock.Verify(x => x.Run(_cart, It.IsAny<PromotionEngineSettings>()), Times.Once);
        }

        [Fact]
        public void AddToCart_WhenLineItemNotInCart_ShouldAddToCart()
        {
            string warningMessage;
            _subject.AddToCart(_cart, "code", out warningMessage);

            Assert.Equal(1, _cart.GetAllLineItems().Single(x => x.Code == "code").Quantity);
        }

        [Fact]
        public void AddToCart_WhenLineItemAlreadyInCart_ShouldIncreaseQuantity()
        {
            string warningMessage;
            _subject.AddToCart(_cart, "code", out warningMessage);
            _subject.AddToCart(_cart, "code", out warningMessage);

            Assert.Equal(2, _cart.GetAllLineItems().Single(x => x.Code == "code").Quantity);
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

            _subject.ChangeCartItem(_cart, shipmentId, code, quantity, size, newSize);

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

            _subject.ChangeCartItem(_cart, shipmentId, code, quantity, size, newSize);

            Assert.Empty(_cart.GetAllLineItems());
        }

        [Fact]
        public void ChangeCartItem_WhenLineItemToChangeToDoesNotExists_ShouldUpdateLineItem()
        {
            _productServiceMock.Setup(x => x.GetSiblingVariantCodeBySize(It.IsAny<string>(), It.IsAny<string>())).Returns("newcode");

            _cart.GetFirstShipment().LineItems.Add(new InMemoryLineItem { Quantity = 1, Code = "code" });

            _subject.ChangeCartItem(_cart, 0, "code", 5, "S", "M");

            Assert.Equal<string>("newcode", _cart.GetFirstShipment().LineItems.Single().Code);
            Assert.Equal<decimal>(5, _cart.GetFirstShipment().LineItems.Single().Quantity);
        }

        [Fact]
        public void ChangeCartItem_WhenLineItemToChangeToAlreadyExists_ShouldUpdateLineItem()
        {
            _productServiceMock.Setup(x => x.GetSiblingVariantCodeBySize(It.IsAny<string>(), It.IsAny<string>())).Returns("newcode");

            _cart.GetFirstShipment().LineItems.Add(new InMemoryLineItem { Quantity = 1, Code = "newcode" });
            _cart.GetFirstShipment().LineItems.Add(new InMemoryLineItem { Quantity = 1, Code = "code" });

            _subject.ChangeCartItem(_cart, 0, "code", 5, "S", "M");

            Assert.Equal<string>("newcode", _cart.GetFirstShipment().LineItems.Single().Code);
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
        public void LoadCart_WhenExistInvalidItems_ShouldReturnCartWithOnlyValidItems()
        {
            var _validCode = "code1";
            var _inValidCode = "code2";

            var lineItem = new InMemoryLineItem { Quantity = 2, Code = _validCode };            
            var lineItem2 = new InMemoryLineItem { Quantity = 2, Code = _inValidCode };
            _cart.GetFirstShipment().LineItems.Add(lineItem);
            _cart.GetFirstShipment().LineItems.Add(lineItem2);

            _lineItemValidatorMock.Setup(x => x.Validate(lineItem, It.IsAny<IMarket>(), It.IsAny<Action<ILineItem, ValidationIssue>>())).Returns(true);
            _lineItemValidatorMock.Setup(x => x.Validate(lineItem2, It.IsAny<IMarket>(), It.IsAny<Action<ILineItem, ValidationIssue>>())).Returns(false);

            var result = _subject.LoadCart(_subject.DefaultCartName);
            
            Assert.True(result.GetAllLineItems().Any(l => l.Code == _validCode));
            Assert.False(result.GetAllLineItems().Any(l => l.Code == _inValidCode));
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

            Assert.Equal<int>(1, _cart.GetAllLineItems().Where(x => x.Code == giftSkuCode && x.IsGift).Count());
            Assert.Equal<int>(2, _cart.GetAllLineItems().Where(x => x.Code == skuCode && !x.IsGift).Count());
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
            Assert.Equal<int>(2, _cart.GetAllLineItems().Where(x => x.Code == skuCode && !x.IsGift).Count());
            Assert.Equal<int>(1, _cart.GetAllLineItems().Where(x => x.Code == skuCode && x.IsGift).Count());
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
            var expectedResult = new Dictionary<ILineItem, List<ValidationIssue>>();
            var result = _subject.ValidateCart(_cart);

            Assert.Equal<Dictionary<ILineItem, List<ValidationIssue>>>(expectedResult, result);
        }

        private readonly Mock<IAddressBookService> _addressBookServiceMock;
        private readonly Mock<CustomerContextFacade> _customerContextFacaceMock;
        private readonly Mock<IOrderGroupFactory> _orderGroupFactoryMock;
        private readonly Mock<IInventoryProcessor> _inventoryProcessorMock;
        private readonly Mock<ILineItemValidator> _lineItemValidatorMock;
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IPlacedPriceProcessor> _placedPriceProcessorMock;
        private readonly Mock<IPricingService> _pricingServiceMock;
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<IPromotionEngine> _promotionEngineMock;
        private readonly Mock<ICurrentMarket> _currentMarketMock;
        private readonly Mock<IMarket> _marketMock;
        private readonly Mock<ICurrencyService> _currencyServiceMock;
        private readonly CartService _subject;
        private readonly ICart _cart;

        public CartServiceTests()
        {
            _addressBookServiceMock = new Mock<IAddressBookService>();
            _customerContextFacaceMock = new Mock<CustomerContextFacade>();
            _orderGroupFactoryMock = new Mock<IOrderGroupFactory>();
            _inventoryProcessorMock = new Mock<IInventoryProcessor>();
            _lineItemValidatorMock = new Mock<ILineItemValidator>();
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _placedPriceProcessorMock = new Mock<IPlacedPriceProcessor>();
            _pricingServiceMock = new Mock<IPricingService>();
            _productServiceMock = new Mock<IProductService>();
            _productServiceMock = new Mock<IProductService>();
            _promotionEngineMock = new Mock<IPromotionEngine>();
            _marketMock = new Mock<IMarket>();
            _currentMarketMock = new Mock<ICurrentMarket>();
            _currencyServiceMock = new Mock<ICurrencyService>();
            _subject = new CartService(_productServiceMock.Object, _pricingServiceMock.Object, _orderGroupFactoryMock.Object, 
                _customerContextFacaceMock.Object, _placedPriceProcessorMock.Object, _inventoryProcessorMock.Object, 
                _lineItemValidatorMock.Object, _orderRepositoryMock.Object, _promotionEngineMock.Object, 
                _addressBookServiceMock.Object, _currentMarketMock.Object, _currencyServiceMock.Object);
            _cart = new FakeCart(new Mock<IMarket>().Object, new Currency("USD")) { Name = _subject.DefaultCartName };

            _orderGroupFactoryMock.Setup(x => x.CreateLineItem(It.IsAny<string>(), It.IsAny<IOrderGroup>())).Returns((string code, IOrderGroup group) => new FakeLineItem() { Code = code });
            _orderGroupFactoryMock.Setup(x => x.CreateShipment(It.IsAny<IOrderGroup>())).Returns((IOrderGroup orderGroup) => new FakeShipment());
            _orderRepositoryMock.Setup(x => x.Load<ICart>(It.IsAny<Guid>(), _subject.DefaultCartName)).Returns(new[] { _cart });
            _orderRepositoryMock.Setup(x => x.Create<ICart>(It.IsAny<Guid>(), _subject.DefaultCartName)).Returns(_cart);
            _currentMarketMock.Setup(x => x.GetCurrentMarket()).Returns(_marketMock.Object);
            _lineItemValidatorMock.Setup(x => x.Validate(It.IsAny<ILineItem>(), It.IsAny<IMarket>(), It.IsAny<Action<ILineItem, ValidationIssue>>())).Returns(true);
            _placedPriceProcessorMock.Setup(x => x.UpdatePlacedPrice(It.IsAny<ILineItem>(), It.IsAny<CustomerContact>(), It.IsAny<IMarket>(), _cart.Currency, It.IsAny<Action<ILineItem, ValidationIssue>>())).Returns(true);
        }

        class PromotionForTest : EntryPromotion
        {
        }
    }
}
