using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Internal;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Models;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Services;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes;
using FluentAssertions;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Cart.ViewModelFactories
{
    public class ShipmentViewModelFactoryTests
    {
        [Fact]
        public void CreateShipmentsViewModel_ShouldReturnViewModel()
        {
            var viewModel = _subject.CreateShipmentsViewModel(_cart).First();

            var expectedViewModel = new ShipmentViewModel
            {
                Address = _addressModel,
                CartItems = new List<CartItemViewModel> { _cartItem },
                ShippingMethods = new List<ShippingMethodViewModel> { new ShippingMethodViewModel { Id = _shippingRate.Id, DisplayName = _shippingRate.Name, Price = _shippingRate.Money } },
                ShippingMethodId = _shippingRate.Id
            };

            viewModel.Should().BeEquivalentTo(expectedViewModel);
        }

        [Fact]
        public void CreateShipmentsViewModel_WithoutAvailableShippingMethod_ShouldNotThrowException()
        {
            _shippingManagerFacadeMock.Setup(x => x.GetShippingMethodsByMarket(It.IsAny<string>(), It.IsAny<bool>())).Returns(new List<ShippingMethodInfoModel>());
            var viewModel = _subject.CreateShipmentsViewModel(_cart).First();
            Assert.Equal(viewModel.ShippingMethodId, Guid.Empty);
        }

        private readonly ShipmentViewModelFactory _subject;
        private readonly ICart _cart;
        private readonly AddressModel _addressModel;
        private readonly CartItemViewModel _cartItem;
        private readonly ShippingRate _shippingRate;

        private readonly Mock<ShippingManagerFacade> _shippingManagerFacadeMock;
        private readonly Mock<IMarketService> _marketServiceMock;

        public ShipmentViewModelFactoryTests()
        {
            var market = new MarketImpl(new MarketId(Currency.USD));
            _cart = new FakeCart(market, Currency.USD) { Name = "Default" };
            _cart.Forms.Single().Shipments.Single().LineItems.Add(new InMemoryLineItem { Code = "code" });
            _cart.Forms.Single().CouponCodes.Add("couponcode");

            _shippingManagerFacadeMock = new Mock<ShippingManagerFacade>(null, null);
            _shippingManagerFacadeMock.Setup(x => x.GetShippingMethodsByMarket(It.IsAny<string>(), It.IsAny<bool>())).Returns(() => new List<ShippingMethodInfoModel>
            {
                new ShippingMethodInfoModel
                {
                    LanguageId = CultureInfo.InvariantCulture.TwoLetterISOLanguageName,
                    Currency = Currency.USD
                }
            });
            _shippingRate = new ShippingRate(Guid.NewGuid(), "name", new Money(10, Currency.USD));
            _shippingManagerFacadeMock.Setup(x => x.GetRate(It.IsAny<IShipment>(), It.IsAny<ShippingMethodInfoModel>(), It.IsAny<IMarket>()))
                .Returns(_shippingRate);

            var languageServiceMock = new Mock<LanguageService>(null, null, null);
            languageServiceMock.Setup(x => x.GetCurrentLanguage()).Returns(CultureInfo.InvariantCulture);

            var addressBookServiceMock = new Mock<IAddressBookService>();
            _addressModel = new AddressModel();
            addressBookServiceMock.Setup(x => x.ConvertToModel(It.IsAny<IOrderAddress>())).Returns(_addressModel);

            _cartItem = new CartItemViewModel();
            var cartItemViewModelFactoryMock = new Mock<CartItemViewModelFactory>(null, null, null, null, null, null);
            cartItemViewModelFactoryMock.Setup(x => x.CreateCartItemViewModel(It.IsAny<ICart>(), It.IsAny<ILineItem>(), It.IsAny<VariationContent>())).Returns(_cartItem);

            var catalogContentServiceMock = new Mock<CatalogContentService>(null, null, null, null, null, null, null);
            catalogContentServiceMock.Setup(x => x.GetItems<EntryContentBase>(It.IsAny<IEnumerable<string>>()))
                .Returns(() => new List<VariationContent> { new VariationContent { Code = "code" } });

            _marketServiceMock = new Mock<IMarketService>();
            _marketServiceMock.Setup(x => x.GetMarket(It.IsAny<MarketId>())).Returns(market);
            _subject = new ShipmentViewModelFactory(
                catalogContentServiceMock.Object,
                _shippingManagerFacadeMock.Object,
                languageServiceMock.Object,
                addressBookServiceMock.Object,
                cartItemViewModelFactoryMock.Object,
                _marketServiceMock.Object);
        }
    }
}
