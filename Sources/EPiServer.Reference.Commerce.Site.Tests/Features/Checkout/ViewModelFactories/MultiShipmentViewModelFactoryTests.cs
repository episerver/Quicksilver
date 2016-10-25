using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Internal;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Tests.Features.Cart;
using EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Markets;
using Moq;
using Xunit;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Checkout.ViewModelFactories
{
    public class MultiShipmentViewModelFactoryTests
    {
        [Fact]
        public void CreateMultiShipmentViewModel_WhenAuthenticated_ShouldReturnViewModel()
        {
            var viewModel = _subject.CreateMultiShipmentViewModel(_cart, true);

            Assert.Equal(viewModel.StartPage, _startPage);
            Assert.NotNull(viewModel.AvailableAddresses.SingleOrDefault(x => x.AddressId == "addressid"));
            Assert.True(viewModel.ReferrerUrl.Contains("http://site.com"));
            Assert.Equal(viewModel.CartItems.Length, 1);
        }

        [Fact]
        public void CreateMultiShipmentViewModel_WhenNotAuthenticated_ShouldReturnViewModel()
        {
            var viewModel = _subject.CreateMultiShipmentViewModel(_cart, false);

            Assert.Equal(viewModel.StartPage, _startPage);
            Assert.NotNull(viewModel.AvailableAddresses.SingleOrDefault(x => x.AddressId == "addressid"));
            Assert.Equal(viewModel.AvailableAddresses.Count, 2);
            Assert.True(viewModel.ReferrerUrl.Contains("http://site.com"));
            Assert.Equal(viewModel.CartItems.Length, 1);
        }

        private readonly MultiShipmentViewModelFactory _subject;
        private readonly ICart _cart;
        private readonly StartPage _startPage;

        public MultiShipmentViewModelFactoryTests()
        {
            _cart = new FakeCart(new MarketImpl(new MarketId(Currency.USD)), Currency.USD);
            _cart.Forms.Single().Shipments.Single().LineItems.Add(new InMemoryLineItem ());
            _cart.Forms.Single().CouponCodes.Add("couponcode");

            var addressBookServiceMock = new Mock<IAddressBookService>();
            addressBookServiceMock.Setup(x => x.List()).Returns(() => new List<AddressModel> { new AddressModel { AddressId = "addressid" } });
            var preferredBillingAddress = CustomerAddress.CreateInstance();
            preferredBillingAddress.Name = "preferredBillingAddress";
            addressBookServiceMock.Setup(x => x.GetPreferredBillingAddress()).Returns(preferredBillingAddress);
            addressBookServiceMock.Setup(x => x.UseBillingAddressForShipment()).Returns(true);

            _startPage = new StartPage();
            var contentLoaderMock = new Mock<IContentLoader>();
            contentLoaderMock.Setup(x => x.Get<StartPage>(It.IsAny<PageReference>())).Returns(_startPage);

            var urlResolverMock = new Mock<UrlResolver>();

            var httpcontextMock = new Mock<HttpContextBase>();
            var requestMock = new Mock<HttpRequestBase>();
            requestMock.Setup(x => x.Url).Returns(new Uri("http://site.com"));
            requestMock.Setup(x => x.UrlReferrer).Returns(new Uri("http://site.com"));
            httpcontextMock.Setup(x => x.Request).Returns(requestMock.Object);

            PreferredCultureAccessor accessor = () => CultureInfo.InvariantCulture;
            var shipmentViewModelFactoryMock = new Mock<ShipmentViewModelFactory>(null, null, null, null, null, null, accessor, null);
            shipmentViewModelFactoryMock.Setup(x => x.CreateShipmentsViewModel(It.IsAny<ICart>())).Returns(() => new[]
            {
                new ShipmentViewModel {CartItems = new[]
                {
                    new CartItemViewModel { Quantity = 1 }
                }
            }
            });

            _subject = new MultiShipmentViewModelFactory(
                new MemoryLocalizationService(),
                addressBookServiceMock.Object,
                contentLoaderMock.Object,
                urlResolverMock.Object,
                (() => httpcontextMock.Object),
                shipmentViewModelFactoryMock.Object);
        }
    }
}
