using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Internal;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes;
using FluentAssertions;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using Moq;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EPiServer.Globalization;
using Mediachase.Commerce.Catalog;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Cart.ViewModelFactories
{
    public class CartViewModelFactoryTests
    {
        [Fact]
        public void CreateMiniCartViewModel_ShouldCreateViewModel()
        {
            var viewModel = _subject.CreateMiniCartViewModel(_cart);

            var expectedViewModel = new MiniCartViewModel
            {
                ItemCount = 1,
                CheckoutPage = _startPage.CheckoutPage,
                Shipments = new[] { new ShipmentViewModel { CartItems = _cartItems } },
                Total = _totals.SubTotal
            };

            viewModel.ShouldBeEquivalentTo(expectedViewModel);
        }

        [Fact]
        public void CreateMiniCartViewModel_WhenItemIsDeleted_ShouldNotCountIt()
        {
            _cart.Forms.Single().Shipments.Single().LineItems.Add(new InMemoryLineItem { Code = "Deleted", Quantity = 1, PlacedPrice = 100 });
            _referenceConverterMock.Setup(c => c.GetContentLink(It.Is<string>(code => code == "Deleted"))).Returns(ContentReference.EmptyReference);
            var viewModel = _subject.CreateMiniCartViewModel(_cart);

            var expectedViewModel = new MiniCartViewModel
            {
                ItemCount = 1,
                CheckoutPage = _startPage.CheckoutPage,
                Shipments = new[] { new ShipmentViewModel { CartItems = _cartItems } },
                Total = _totals.SubTotal
            };

            viewModel.ShouldBeEquivalentTo(expectedViewModel);
        }

        [Fact]
        public void CreateMiniCartViewModel_WhenCartIsNull_ShouldCreateViewModel()
        {
            var viewModel = _subject.CreateMiniCartViewModel(null);

            var expectedViewModel = new MiniCartViewModel
            {
                ItemCount = 0,
                CheckoutPage = _startPage.CheckoutPage,
                Shipments = Enumerable.Empty<ShipmentViewModel>(),
                Total = new Money(0, Currency.USD)
            };

            viewModel.ShouldBeEquivalentTo(expectedViewModel);
        }

        [Fact]
        public void CreateLargeCartViewModel_ShouldCreateViewModel()
        {
            var viewModel = _subject.CreateLargeCartViewModel(_cart);

            var expectedViewModel = new LargeCartViewModel
            {
                Shipments = new[] { new ShipmentViewModel { CartItems = _cartItems } },
                Total = new Money(100, Currency.USD),
                TotalDiscount = _orderDiscountTotal,
            };

            viewModel.ShouldBeEquivalentTo(expectedViewModel);
        }

        [Fact]
        public void CreateLargeCartViewModel_WhenCartIsNull_ShouldCreateViewModel()
        {
            var viewModel = _subject.CreateLargeCartViewModel(null);

            var expectedViewModel = new LargeCartViewModel
            {
                Shipments = Enumerable.Empty<ShipmentViewModel>(),
                Total = new Money(0, Currency.USD),
                TotalDiscount = new Money(0, Currency.USD)
            };

            viewModel.ShouldBeEquivalentTo(expectedViewModel);
        }

        [Fact]
        public void CreateWishListViewModel_ShouldCreateViewModel()
        {
            var viewModel = _subject.CreateWishListViewModel(_cart);

            var expectedViewModel = new WishListViewModel
            {
                ItemCount = 1,
                CartItems = _cartItems,
                Total = _totals.SubTotal
            };

            viewModel.ShouldBeEquivalentTo(expectedViewModel);
        }

        [Fact]
        public void CreateWishListViewModel_WhenCartIsNull_ShouldCreateViewModel()
        {
            var viewModel = _subject.CreateWishListViewModel(null);

            var expectedViewModel = new WishListViewModel
            {
                ItemCount = 0,
                CartItems = new CartItemViewModel[0],
                Total = new Money(0, Currency.USD)
            };

            viewModel.ShouldBeEquivalentTo(expectedViewModel);
        }

        [Fact]
        public void CreateWishListMiniCartViewModel_ShouldCreateViewModel()
        {
            var viewModel = _subject.CreateWishListMiniCartViewModel(_cart);

            var expectedViewModel = new WishListMiniCartViewModel
            {
                ItemCount = 1,
                WishListPage = _startPage.WishListPage,
                CartItems = _cartItems,
                Total = _totals.SubTotal
            };

            viewModel.ShouldBeEquivalentTo(expectedViewModel);
        }

        [Fact]
        public void CreateWishListMiniCartViewModel_WhenCartIsNull_ShouldCreateViewModel()
        {
            var viewModel = _subject.CreateWishListMiniCartViewModel(null);

            var expectedViewModel = new WishListMiniCartViewModel
            {
                ItemCount = 0,
                WishListPage = _startPage.WishListPage,
                CartItems = new CartItemViewModel[0],
                Total = new Money(0, Currency.USD)
            };

            viewModel.ShouldBeEquivalentTo(expectedViewModel);
        }

        private readonly CartViewModelFactory _subject;
        private readonly StartPage _startPage;
        private readonly IList<CartItemViewModel> _cartItems;
        private readonly ICart _cart;
        private readonly OrderGroupTotals _totals;
        private readonly Money _orderDiscountTotal;
        private readonly Mock<ReferenceConverter> _referenceConverterMock;

        public CartViewModelFactoryTests()
        {
            _cart = new FakeCart(new MarketImpl(MarketId.Default), Currency.USD);
            _cart.Forms.Single().Shipments.Single().LineItems.Add(new InMemoryLineItem { Quantity = 1, PlacedPrice = 105, LineItemDiscountAmount = 5 });

            _startPage = new StartPage() { CheckoutPage = new ContentReference(1), WishListPage = new ContentReference(1) };
            var contentLoaderMock = new Mock<IContentLoader>();
            contentLoaderMock.Setup(x => x.Get<StartPage>(It.IsAny<ContentReference>())).Returns(_startPage);
            var languageResolverMock = new Mock<LanguageResolver>();
            languageResolverMock.Setup(x => x.GetPreferredCulture()).Returns(CultureInfo.InvariantCulture);

            var shipmentViewModelFactoryMock = new Mock<ShipmentViewModelFactory>(null, null, null, null, null, null, languageResolverMock.Object);
            _cartItems = new List<CartItemViewModel> { new CartItemViewModel { DiscountedPrice = new Money(100, Currency.USD), Quantity = 1 } };
            shipmentViewModelFactoryMock.Setup(x => x.CreateShipmentsViewModel(It.IsAny<ICart>())).Returns(() => new[] { new ShipmentViewModel { CartItems = _cartItems } });

            _referenceConverterMock = new Mock<ReferenceConverter>(null, null);
            _referenceConverterMock.Setup(c => c.GetContentLink(It.IsAny<string>())).Returns(new ContentReference(1));

            var currencyServiceMock = new Mock<ICurrencyService>();
            currencyServiceMock.Setup(x => x.GetCurrentCurrency()).Returns(Currency.USD);

            _totals = new OrderGroupTotals(
                new Money(100, Currency.USD),
                new Money(100, Currency.USD),
                new Money(100, Currency.USD),
                new Money(100, Currency.USD),
                new Money(100, Currency.USD),
                new Dictionary<IOrderForm, OrderFormTotals>());

            _orderDiscountTotal = new Money(5, Currency.USD);
            var orderGroupCalculatorMock = new Mock<IOrderGroupCalculator>();
            orderGroupCalculatorMock.Setup(x => x.GetOrderDiscountTotal(It.IsAny<IOrderGroup>(), It.IsAny<Currency>()))
                .Returns(_orderDiscountTotal);

            orderGroupCalculatorMock.Setup(x => x.GetSubTotal(_cart)).Returns(new Money(_cart.GetAllLineItems().Sum(x => x.PlacedPrice * x.Quantity - ((ILineItemDiscountAmount)x).EntryAmount), _cart.Currency));

            _subject = new CartViewModelFactory(
                contentLoaderMock.Object,
                currencyServiceMock.Object,
                orderGroupCalculatorMock.Object,
                shipmentViewModelFactoryMock.Object,
                _referenceConverterMock.Object);
        }
    }
}
