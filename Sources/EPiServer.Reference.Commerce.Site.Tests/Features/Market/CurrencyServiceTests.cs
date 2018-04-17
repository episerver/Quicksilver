using System.Linq;
using Mediachase.Commerce;
using Moq;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Market
{
    public class CurrencyServiceTests
    {
        [Fact]
        public void GetAvailableCurrencies_ShouldReturnAllCurrenciesForCurrentMarket()
        {
            _marketMock.Setup(x => x.Currencies).Returns(() => new[] { new Currency("USD"), new Currency("SEK") });
          
            var currencies = _subject.GetAvailableCurrencies();

            Assert.Equal<int>(2, currencies.Count());
        }

        [Fact]
        public void GetCurrentCurrency_WhenCookieDoesNotExist_ShouldReturnDefaultCurrency()
        {
            _cookieServiceMock.Setup(x => x.Get(It.IsAny<string>())).Returns(() => null);
            _marketMock.Setup(x => x.DefaultCurrency).Returns(new Currency("USD"));

            var currentCurrency = _subject.GetCurrentCurrency();

            Assert.Equal<Currency>(new Currency("USD"), currentCurrency);
        }

        [Fact]
        public void GetCurrentCurrency_WhenCookieExist_ShouldReturnTheCookieValue()
        {
            _cookieServiceMock.Setup(x => x.Get(It.IsAny<string>())).Returns("SEK");
            _marketMock.Setup(x => x.DefaultCurrency).Returns(new Currency("USD"));

            var currentCurrency = _subject.GetCurrentCurrency();

            Assert.Equal<Currency>(new Currency("SEK"), currentCurrency);
        }

        [Fact]
        public void SetCurrentCurrency_WhenCurrencyDoesNotExist_ReturnsFalse()
        {
            _marketMock.Setup(x => x.Currencies).Returns(() => new[] { new Currency("GBP"), new Currency("SEK") });

            var result = _subject.SetCurrentCurrency("USD");
            
            Assert.False(result);
        }

        [Fact]
        public void SetCurrentCurrency_WhenCurrencyExists_ReturnsTrue()
        {
            _marketMock.Setup(x => x.Currencies).Returns(() => new[] { new Currency("GBP"), new Currency("SEK") });

            var result = _subject.SetCurrentCurrency("GBP");

            Assert.True(result);
        }

        private readonly Mock<ICurrentMarket> _currentMarketMock;
        private readonly Mock<IMarket> _marketMock;
        private readonly Mock<CookieService> _cookieServiceMock;
        private readonly CurrencyService _subject;

        public CurrencyServiceTests()
        {
            _marketMock = new Mock<IMarket>();
            _marketMock.Setup(x => x.Currencies).Returns(() => new[] { new Currency("USD"), new Currency("SEK") });
            _marketMock.Setup(x => x.DefaultCurrency).Returns(new Currency("USD"));

            _cookieServiceMock = new Mock<CookieService>();
            _cookieServiceMock.Setup(x => x.Get(It.IsAny<string>())).Returns("USD");

            _currentMarketMock = new Mock<ICurrentMarket>();
            _currentMarketMock.Setup(x => x.GetCurrentMarket()).Returns(_marketMock.Object);

            _subject = new CurrencyService(_currentMarketMock.Object, _cookieServiceMock.Object);
        }
    }
}
