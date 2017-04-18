using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;
using EPiServer.Reference.Commerce.Site.Features.Payment.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels;
using Mediachase.Commerce;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Payment.ViewModelFactories
{
    public class PaymentMethodViewModelFactoryTests
    {
        [Fact]
        public void GetPaymentMethods_WhenPaymentMethodExists_ShouldReturnViewModels()
        {
            var result = _subject.GetPaymentMethodViewModels();
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetPaymentMethods_WhenNoPaymentMethodExists_ShouldReturnEmptyList()
        {
            _languageServiceMock.Setup(x => x.GetCurrentLanguage()).Returns(new CultureInfo("sv-SE"));

            var result = _subject.GetPaymentMethodViewModels();
            var expected = new List<PaymentMethodViewModel>();
            Assert.Equal<IEnumerable<PaymentMethodViewModel>>(expected, result);
        }

        [Fact]
        public void GetPaymentMethods_WhenSetDefaultPaymentMethodToFirstPaymentMethod_ShouldReturnDefaultPaymentMethodCorrectly()
        {
            var result = _subject.GetPaymentMethodViewModels().FirstOrDefault().IsDefault;
            var expected = true;

            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetPaymentMethods_WhenSetDefaultPaymentMethodToOtherPaymentMethod_ShouldReturnDefaultPaymentMethodCorrectly()
        {
            _paymentServiceMock.Setup(x => x.GetPaymentMethodsByMarketIdAndLanguageCode(It.IsAny<string>(), "en")).Returns(
                new[]
                {
                    new PaymentMethodViewModel { PaymentMethodId = Guid.NewGuid(), Description = "Lorem ipsum", FriendlyName = "name 1", SystemKeyword = "CashOnDelivery" },
                    new PaymentMethodViewModel { PaymentMethodId = Guid.NewGuid(), Description = "Lorem ipsum", FriendlyName = "name 2", SystemKeyword = "GenericCreditCard" , IsDefault = true},
                    new PaymentMethodViewModel { PaymentMethodId = Guid.NewGuid(), Description = "Lorem ipsum", FriendlyName = "name 3", SystemKeyword = "system3" }
                });

            var cashOnDeliveryPaymentOption = new CashOnDeliveryPaymentOption(null, null, _currentMarketMock.Object, _languageServiceMock.Object, _paymentServiceMock.Object);
            var creaditPaymentOption = new GenericCreditCardPaymentOption(null, null, _currentMarketMock.Object, _languageServiceMock.Object, _paymentServiceMock.Object);

            _paymentOptions = new List<IPaymentOption>();
            _paymentOptions.Add(cashOnDeliveryPaymentOption as IPaymentOption);
            _paymentOptions.Add(creaditPaymentOption as IPaymentOption);

            _subject = new PaymentMethodViewModelFactory(_currentMarketMock.Object, _languageServiceMock.Object, _paymentServiceMock.Object, _paymentOptions);

            var result = _subject.GetPaymentMethodViewModels().FirstOrDefault(p => p.SystemKeyword == "GenericCreditCard").IsDefault;
            var expected = true;

            Assert.Equal(expected, result);
        }

        private PaymentMethodViewModelFactory _subject;
        private readonly Mock<IPaymentService> _paymentServiceMock;
        private readonly Mock<LanguageService> _languageServiceMock;
        private readonly Mock<ICurrentMarket> _currentMarketMock;
        private readonly Mock<IMarket> _marketMock;
        private List<IPaymentOption> _paymentOptions;

        public PaymentMethodViewModelFactoryTests()
        {
            _paymentServiceMock = new Mock<IPaymentService>();
            _paymentServiceMock.Setup(x => x.GetPaymentMethodsByMarketIdAndLanguageCode(It.IsAny<string>(), "en")).Returns(
                new[]
                {
                    new PaymentMethodViewModel { PaymentMethodId = Guid.NewGuid(), Description = "Lorem ipsum", FriendlyName = "name 1", SystemKeyword = "CashOnDelivery" , IsDefault = true},
                    new PaymentMethodViewModel { PaymentMethodId = Guid.NewGuid(), Description = "Lorem ipsum", FriendlyName = "name 2", SystemKeyword = "GenericCreditCard" },
                    new PaymentMethodViewModel { PaymentMethodId = Guid.NewGuid(), Description = "Lorem ipsum", FriendlyName = "name 3", SystemKeyword = "system3" }
                });

            _marketMock = new Mock<IMarket>();
            _marketMock.Setup(m => m.MarketId).Returns(new MarketId("sampleMarket"));

            _currentMarketMock = new Mock<ICurrentMarket>();
            _currentMarketMock.Setup(x => x.GetCurrentMarket()).Returns(_marketMock.Object);

            _languageServiceMock = new Mock<LanguageService>(null, null, null);
            _languageServiceMock.Setup(x => x.GetCurrentLanguage()).Returns(new CultureInfo("en-US"));

            var cashOnDeliveryPaymentOption = new CashOnDeliveryPaymentOption(null, null, _currentMarketMock.Object, _languageServiceMock.Object, _paymentServiceMock.Object);
            var creaditPaymentOption = new GenericCreditCardPaymentOption(null, null, _currentMarketMock.Object, _languageServiceMock.Object, _paymentServiceMock.Object);

            _paymentOptions = new List<IPaymentOption>();
            _paymentOptions.Add(cashOnDeliveryPaymentOption as IPaymentOption);
            _paymentOptions.Add(creaditPaymentOption as IPaymentOption);
            
            _subject = new PaymentMethodViewModelFactory(_currentMarketMock.Object, _languageServiceMock.Object, _paymentServiceMock.Object, _paymentOptions);
        }
    }
}