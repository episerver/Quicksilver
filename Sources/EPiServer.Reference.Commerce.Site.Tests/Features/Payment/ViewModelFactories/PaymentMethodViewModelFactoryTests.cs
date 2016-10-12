using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.Models;
using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;
using EPiServer.Reference.Commerce.Site.Features.Payment.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels;
using Mediachase.Commerce;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
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
            var expected = new List<PaymentMethodViewModel<PaymentMethodBase>>();
            Assert.Equal<IEnumerable<PaymentMethodViewModel<PaymentMethodBase>>>(expected, result);
        }

        private readonly PaymentMethodViewModelFactory _subject;
        private readonly Mock<IPaymentService> _paymentServiceMock;
        private readonly Mock<LanguageService> _languageServiceMock;
        private readonly Mock<ICurrentMarket> _currentMarketMock;
        private readonly Mock<IMarket> _marketMock;

        public PaymentMethodViewModelFactoryTests()
        {
            _paymentServiceMock = new Mock<IPaymentService>();
            _marketMock = new Mock<IMarket>();
            _currentMarketMock = new Mock<ICurrentMarket>();
            _languageServiceMock = new Mock<LanguageService>(null, null, null, null);

            _currentMarketMock.Setup(x => x.GetCurrentMarket()).Returns(_marketMock.Object);
            _languageServiceMock.Setup(x => x.GetCurrentLanguage()).Returns(new CultureInfo("en-US"));
            _paymentServiceMock.Setup(x => x.GetPaymentMethodsByMarketIdAndLanguageCode(It.IsAny<string>(), "en")).Returns(
                new[]
                {
                    new PaymentMethodModel { Description = "Lorem ipsum", FriendlyName = "name 1", LanguageId = "en", PaymentMethodId = Guid.NewGuid(), SystemName = "system 1" },
                    new PaymentMethodModel { Description = "Lorem ipsum", FriendlyName = "name 2", LanguageId = "en", PaymentMethodId = Guid.NewGuid(), SystemName = "system 2" },
                    new PaymentMethodModel { Description = "Lorem ipsum", FriendlyName = "name 3", LanguageId = "en", PaymentMethodId = Guid.NewGuid(), SystemName = "system 3" }
                });

            _subject = new PaymentMethodViewModelFactory(_currentMarketMock.Object, _languageServiceMock.Object, _paymentServiceMock.Object);
        }
    }
}