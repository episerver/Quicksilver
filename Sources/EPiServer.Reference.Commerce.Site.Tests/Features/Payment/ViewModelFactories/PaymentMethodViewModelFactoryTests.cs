using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels;
using EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes;
using Mediachase.Commerce;
using Moq;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Payment.ViewModelFactories
{
    public class PaymentMethodViewModelFactoryTests
    {
        [Fact]
        public void CreatePaymenMethodSelectionViewModel_WhenPaymentMethodExists_ShouldReturnViewModels()
        {
            var result = _subject.CreatePaymentMethodSelectionViewModel(null);

            Assert.NotEmpty(result.PaymentMethods);
        }

        [Fact]
        public void CreatePaymenMethodSelectionViewModel_WhenNoPaymentMethodExists_ShouldReturnEmptyList()
        {
            _languageServiceMock.Setup(x => x.GetCurrentLanguage()).Returns(new CultureInfo("sv-SE"));

            var result = _subject.CreatePaymentMethodSelectionViewModel(null);
            var expected = new List<PaymentMethodViewModel<IPaymentMethod>>();

            Assert.Equal<IEnumerable<PaymentMethodViewModel<IPaymentMethod>>>(expected, result.PaymentMethods);
        }

        [Fact()]
        public void CreatePaymenMethodSelectionViewModel_WhenNoDefaultPaymentMethod_ShouldSetFirstPaymentMethodAsSelected()
        {
            foreach (var method in _paymentMethods)
            {
                method.IsDefault = false;
            }

            var result = _subject.CreatePaymentMethodSelectionViewModel(null);
            var expected = result.PaymentMethods.First().PaymentMethod;

            Assert.Equal(expected, result.SelectedPaymentMethod.PaymentMethod);
        }

        [Fact]
        public void CreatePaymenMethodSelectionViewModel_WhenDefaultPaymentMethod_ShouldReturnDefaultPaymentMethodCorrectly()
        {
            var result = _subject.CreatePaymentMethodSelectionViewModel(null);
            var expected = result.PaymentMethods.First(method => method.IsDefault == true).PaymentMethod;
            var actual = result.SelectedPaymentMethod.PaymentMethod;

            Assert.Equal(expected, actual);
        }

        private PaymentMethodViewModelFactory _subject;
        private readonly Mock<IPaymentManagerFacade> _paymentManagerMock;
        private readonly Mock<LanguageService> _languageServiceMock;
        private readonly Mock<ICurrentMarket> _currentMarketMock;
        private readonly Mock<IMarket> _marketMock;
        private readonly IEnumerable<PaymentMethodViewModel<IPaymentMethod>> _paymentMethods;

        public PaymentMethodViewModelFactoryTests()
        {
            _paymentMethods = new[]
            {
                new PaymentMethodViewModel<IPaymentMethod> { IsDefault = false, PaymentMethod = new FakePaymentMethod("FirstOption")},
                new PaymentMethodViewModel<IPaymentMethod>{ IsDefault = true, PaymentMethod = new FakePaymentMethod("SecondOption")},
                new PaymentMethodViewModel<IPaymentMethod>{ IsDefault = false, PaymentMethod = new FakePaymentMethod("ThirdOption")}
            };

            _paymentManagerMock = new Mock<IPaymentManagerFacade>();
            _paymentManagerMock.Setup(x => x.GetPaymentMethodsByMarket(It.IsAny<string>()))
                .Returns(() =>
                {
                    var dto = new Mediachase.Commerce.Orders.Dto.PaymentMethodDto();
                    var ordering = 0;
                    dto.EnforceConstraints = false;

                    foreach (var method in _paymentMethods)
                    {
                        var row = dto.PaymentMethod.NewPaymentMethodRow();
                        row.SystemKeyword = method.PaymentMethod.SystemKeyword;
                        row.PaymentMethodId = method.PaymentMethod.PaymentMethodId;
                        row.Name = method.PaymentMethod.Name;
                        row.LanguageId = "en";
                        row.IsActive = true;
                        row.IsDefault = method.IsDefault;
                        row.Ordering = ++ordering;
                        dto.PaymentMethod.AddPaymentMethodRow(row);
                    }

                    return dto;
                });


            _marketMock = new Mock<IMarket>();
            _marketMock.Setup(m => m.MarketId).Returns(new MarketId("sampleMarket"));

            _currentMarketMock = new Mock<ICurrentMarket>();
            _currentMarketMock.Setup(x => x.GetCurrentMarket()).Returns(_marketMock.Object);

            _languageServiceMock = new Mock<LanguageService>(null, null, null);
            _languageServiceMock.Setup(x => x.GetCurrentLanguage()).Returns(new CultureInfo("en-US"));

            _subject = new PaymentMethodViewModelFactory(_currentMarketMock.Object, _languageServiceMock.Object, _paymentMethods.Select(x => x.PaymentMethod), _paymentManagerMock.Object);
        }
    }
}