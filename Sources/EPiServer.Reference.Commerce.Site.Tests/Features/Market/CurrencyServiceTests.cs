using System.Linq;
using Mediachase.Commerce;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Market
{
    [TestClass]
    public class CurrencyServiceTests
    {
        [TestMethod]
        public void GetAvailableCurrencies_ShouldReturnAllCurrenciesForCurrentMarket()
        {
            _marketMock.Setup(x => x.Currencies).Returns(() => new[] { new Currency("USD"), new Currency("SEK") });
          
            var currencies = _subject.GetAvailableCurrencies();

            Assert.AreEqual<int>(2, currencies.Count());
        }

        [TestMethod]
        public void GetCurrentCurrency_WhenCookieDoesNotExist_ShouldReturnDefaultCurrency()
        {
            _cookieServiceMock.Setup(x => x.Get(It.IsAny<string>())).Returns(() => null);
            _marketMock.Setup(x => x.DefaultCurrency).Returns(new Currency("USD"));

            var currentCurrency = _subject.GetCurrentCurrency();

            Assert.AreEqual<Currency>(new Currency("USD"), currentCurrency);
        }

        [TestMethod]
        public void GetCurrentCurrency_WhenCookieExist_ShouldReturnTheCookieValue()
        {
            _cookieServiceMock.Setup(x => x.Get(It.IsAny<string>())).Returns("SEK");
            _marketMock.Setup(x => x.DefaultCurrency).Returns(new Currency("USD"));

            var currentCurrency = _subject.GetCurrentCurrency();

            Assert.AreEqual<Currency>(new Currency("SEK"), currentCurrency);
        }

        [TestMethod]
        public void SetCurrentCurrency_WhenCurrencyDoesNotExist_ReturnsFalse()
        {
            _marketMock.Setup(x => x.Currencies).Returns(() => new[] { new Currency("GBP"), new Currency("SEK") });

            var result = _subject.SetCurrentCurrency("USD");
            
            Assert.AreEqual<bool>(false, result);
        }

        [TestMethod]
        public void SetCurrentCurrency_WhenCurrencyExists_ReturnsTrue()
        {
            _marketMock.Setup(x => x.Currencies).Returns(() => new[] { new Currency("GBP"), new Currency("SEK") });

            var result = _subject.SetCurrentCurrency("GBP");

            Assert.IsTrue(result);
        }

        private Mock<ICurrentMarket> _currentMarketMock;
        private Mock<IMarket> _marketMock;
        private Mock<CookieService> _cookieServiceMock;
        private CurrencyService _subject;

        [TestInitialize]
        public void Setup()
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
