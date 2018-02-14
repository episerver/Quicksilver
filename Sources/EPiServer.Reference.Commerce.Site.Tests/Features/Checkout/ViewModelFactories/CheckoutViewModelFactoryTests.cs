using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Internal;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;
using EPiServer.Reference.Commerce.Site.Features.Payment.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Markets;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Checkout.ViewModelFactories
{
    public class CheckoutViewModelFactoryTests
    {
        [Fact]
        public void CreateCheckoutViewModel_ShouldReturnViewModel()
        {
            var checkoutPage = new CheckoutPage();
            var viewModel = _subject.CreateCheckoutViewModel(_cart, checkoutPage);

            Assert.Equal(1, viewModel.Shipments.Count);
            Assert.Equal("couponcode", viewModel.AppliedCouponCodes.Single());
            Assert.NotNull(viewModel.AvailableAddresses.SingleOrDefault(x => x.AddressId == "addressid"));
            Assert.Equal(viewModel.BillingAddress.AddressId, _preferredBillingAddress.Name);
            Assert.Equal(viewModel.CurrentPage, checkoutPage);
            Assert.Equal(viewModel.StartPage, _startPage);
            Assert.Equal(viewModel.PaymentMethodViewModels.Count(), 2);
            Assert.Equal(viewModel.Payment.SystemKeyword, creditPaymentName); // Select default payment method.
            Assert.Equal(viewModel.UseBillingAddressForShipment, true);
            Assert.True(viewModel.ReferrerUrl.Contains("http://site.com"));
            Assert.Equal(viewModel.ViewName, CheckoutViewModel.SingleShipmentCheckoutViewName);
        }

        [Fact]
        public void CreateCheckoutViewModelWithPayment_ShouldReturnViewModelWithCorrectPayment()
        {
            var checkoutPage = new CheckoutPage();
            var viewModel = _subject.CreateCheckoutViewModel(_cart, checkoutPage, _creditPayment);

            Assert.Equal(viewModel.PaymentMethodViewModels.Count(), 2);
            Assert.Equal(viewModel.Payment.SystemKeyword, creditPaymentName);
        }

        [Fact]
        public void CreateCheckoutViewModel_WhenCartIsNull_ShouldReturnEmptyViewModel()
        {
            var checkoutPage = new CheckoutPage();
            var viewModel = _subject.CreateCheckoutViewModel(null, checkoutPage);

            Assert.Empty(viewModel.Shipments);
            Assert.Empty(viewModel.AppliedCouponCodes);
            Assert.Empty(viewModel.AvailableAddresses);
            Assert.Null(viewModel.BillingAddress);
            Assert.Equal(viewModel.CurrentPage, checkoutPage);
            Assert.Equal(viewModel.StartPage, _startPage);
            Assert.Empty(viewModel.PaymentMethodViewModels);
            Assert.Null(viewModel.Payment);
            Assert.Equal(viewModel.ViewName, CheckoutViewModel.SingleShipmentCheckoutViewName);
        }

        private readonly CheckoutViewModelFactory _subject;
        private readonly ICart _cart;
        private readonly CustomerAddress _preferredBillingAddress;
        private readonly IPaymentOption _creditPayment;
        private readonly StartPage _startPage;
        private const string cashPaymentName = "CashOnDelivery";
        private const string creditPaymentName = "GenericCreditCard";
        private Mock<IAddressBookService> _addressBookServiceMock;

        public CheckoutViewModelFactoryTests()
        {
            _cart = new FakeCart(new MarketImpl(new MarketId(Currency.USD)), Currency.USD);
            _cart.Forms.Single().Shipments.Single().LineItems.Add(new InMemoryLineItem());
            _cart.Forms.Single().CouponCodes.Add("couponcode");            

            var paymentServiceMock = new Mock<IPaymentService>();
            paymentServiceMock.Setup(x => x.GetPaymentMethodsByMarketIdAndLanguageCode(It.IsAny<string>(), "en")).Returns(
               new[]
               {
                    new PaymentMethodViewModel() { PaymentMethodId = Guid.NewGuid(), Description = "Lorem ipsum", FriendlyName = "payment method 1", SystemKeyword = cashPaymentName },
                    new PaymentMethodViewModel() { PaymentMethodId = Guid.NewGuid(), Description = "Lorem ipsum", FriendlyName = "payment method 2", SystemKeyword = creditPaymentName, IsDefault = true }
               });

            var marketMock = new Mock<IMarket>();
            var currentMarketMock = new Mock<ICurrentMarket>();
            var languageServiceMock = new Mock<LanguageService>(null, null, null);

            currentMarketMock.Setup(x => x.GetCurrentMarket()).Returns(marketMock.Object);
            languageServiceMock.Setup(x => x.GetCurrentLanguage()).Returns(new CultureInfo("en-US"));
            
            var cashOnDeliveryPaymentOption = new CashOnDeliveryPaymentOption(null, null, currentMarketMock.Object, languageServiceMock.Object, paymentServiceMock.Object);
            var creaditPaymentOption = new GenericCreditCardPaymentOption(null, null, currentMarketMock.Object, languageServiceMock.Object, paymentServiceMock.Object);

            var paymentOptions = new List<IPaymentOption>();
            paymentOptions.Add(cashOnDeliveryPaymentOption as IPaymentOption);
            paymentOptions.Add(creaditPaymentOption as IPaymentOption);
            _creditPayment = creaditPaymentOption;

            var paymentMethodViewModelFactory = new PaymentMethodViewModelFactory(currentMarketMock.Object, languageServiceMock.Object, paymentServiceMock.Object, paymentOptions);
            
            var orderGroupFactoryMock = new Mock<IOrderGroupFactory>();
            orderGroupFactoryMock.Setup(x => x.CreatePayment(It.IsAny<IOrderGroup>())).Returns((IOrderGroup orderGroup) => new FakePayment());
            var serviceLocatorMock = new Mock<IServiceLocator>();
            serviceLocatorMock.Setup(x => x.GetInstance<IOrderGroupFactory>()).Returns(orderGroupFactoryMock.Object);
            ServiceLocator.SetLocator(serviceLocatorMock.Object);

            _addressBookServiceMock = new Mock<IAddressBookService>();
            _addressBookServiceMock.Setup(x => x.List()).Returns(() => new List<AddressModel> { new AddressModel { AddressId = "addressid" } });
            _preferredBillingAddress = CustomerAddress.CreateInstance();
            _preferredBillingAddress.Name = "preferredBillingAddress";
            _addressBookServiceMock.Setup(x => x.GetPreferredBillingAddress()).Returns(_preferredBillingAddress);
            _addressBookServiceMock.Setup(x => x.UseBillingAddressForShipment()).Returns(true);

            _startPage = new StartPage();
            var contentLoaderMock = new Mock<IContentLoader>();
            contentLoaderMock.Setup(x => x.Get<StartPage>(It.IsAny<PageReference>())).Returns(_startPage);

            var urlResolverMock = new Mock<UrlResolver>();
            var httpcontextMock = new Mock<HttpContextBase>();
            var requestMock = new Mock<HttpRequestBase>();

            requestMock.Setup(x => x.Url).Returns(new Uri("http://site.com"));
            requestMock.Setup(x => x.UrlReferrer).Returns(new Uri("http://site.com"));
            httpcontextMock.Setup(x => x.Request).Returns(requestMock.Object);

            var shipmentViewModelFactoryMock = new Mock<ShipmentViewModelFactory>(null, null, null, null, null);
            shipmentViewModelFactoryMock.Setup(x => x.CreateShipmentsViewModel(It.IsAny<ICart>())).Returns(() => new[]
            {
                new ShipmentViewModel {
                    CartItems = new[]
                    {
                        new CartItemViewModel { Quantity = 1 }
                    }
                }
            });

            _subject = new CheckoutViewModelFactory(
                new MemoryLocalizationService(),
                (() => paymentMethodViewModelFactory),
                _addressBookServiceMock.Object,
                contentLoaderMock.Object,
                urlResolverMock.Object,
                (() => httpcontextMock.Object),
                shipmentViewModelFactoryMock.Object);
        }
    }
}
