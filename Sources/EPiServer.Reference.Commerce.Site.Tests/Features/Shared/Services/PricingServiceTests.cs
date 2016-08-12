using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Pricing;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;


namespace EPiServer.Reference.Commerce.Site.Tests.Features.Shared.Services
{
    public class PricingServiceTests
    {
        [Fact]
        public void GetPriceList_WhenCodeIsInvalid_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => _subject.GetPriceList("", new MarketId(), new PriceFilter()));
        }

        [Fact]
        public void GetPriceList_WhenCatalogKeysAreInvalid_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => _subject.GetPriceList( null as IEnumerable<CatalogKey>, new MarketId(), new PriceFilter()));
        }

        [Fact]
        public void GetPriceList_WhenPassingCode_ShouldReturnPriceListOrderedByAmount()
        {
            var prices = _subject.GetPriceList("code", new MarketId(), new PriceFilter());

            Assert.True(prices[0].UnitPrice.Amount < prices[1].UnitPrice.Amount);
        }

        [Fact]
        public void GetPriceList_WhenPassingCatalogKeys_ShouldReturnPriceListOrderedByAmount()
        {
            var catalogKeys = new[]
            {
                new CatalogKey(new FakeAppContext().ApplicationId, "code"),
                new CatalogKey(new FakeAppContext().ApplicationId, "code")
            };

            var prices = _subject.GetPriceList(catalogKeys, new MarketId(), new PriceFilter());

            Assert.True(prices[0].UnitPrice.Amount < prices[1].UnitPrice.Amount);
        }

        [Fact]
        public void GetCurrentPrice_WhenCodeIsInvalid_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => _subject.GetCurrentPrice(""));
        }

        [Fact]
        public void GetCurrentPrice_WhenPricesExist_ShouldReturnCheapestPrice()
        {
            Money? price = _subject.GetCurrentPrice("code");

            Assert.Equal<Money>(_cheapPriceUSD, price.Value);
        }

        [Fact]
        public void GetCurrentPrice_WhenPricesDoNotExist_ShouldReturnNull()
        {
            _priceServiceMock.Setup(
                x =>
                    x.GetPrices(
                        It.IsAny<MarketId>(),
                        It.IsAny<DateTime>(),
                        It.IsAny<CatalogKey>(),
                        It.IsAny<PriceFilter>()))
                .Returns(Enumerable.Empty<IPriceValue>);

            Money? price = _subject.GetCurrentPrice("code");

            Assert.Equal(price.HasValue, false);            
        }

        private Mock<IPriceService> _priceServiceMock;
        private Mock<ICurrentMarket> _currentMarketMock;
        private Mock<ICurrencyService> _currencyServiceMock;
        private PricingService _subject;
        private Money _cheapPriceUSD;
        private Money _expensivePriceGBP;


        public PricingServiceTests()
        {
            _cheapPriceUSD = new Money(1, "USD");
            _expensivePriceGBP = new Money(2, "GBP");

            _currencyServiceMock = new Mock<ICurrencyService>();
            _currencyServiceMock.Setup(x => x.GetCurrentCurrency()).Returns(new Currency("USD"));

            _currentMarketMock = new Mock<ICurrentMarket>();
            _currentMarketMock.Setup(x => x.GetCurrentMarket()).Returns(new Mock<IMarket>().Object);

            _priceServiceMock = new Mock<IPriceService>();
            _priceServiceMock.Setup(
                x =>
                    x.GetPrices(
                        It.IsAny<MarketId>(),
                        It.IsAny<DateTime>(),
                        It.IsAny<CatalogKey>(),
                        It.IsAny<PriceFilter>()))
                .Returns(() => new[]
                {
                    new PriceValue { UnitPrice = _expensivePriceGBP},
                    new PriceValue { UnitPrice = _cheapPriceUSD}
                });
            _priceServiceMock.Setup(
                x =>
                    x.GetPrices(
                        It.IsAny<MarketId>(),
                        It.IsAny<DateTime>(),
                        It.IsAny<IEnumerable<CatalogKey>>(),
                        It.IsAny<PriceFilter>()))
                .Returns(() => new[]
                {
                    new PriceValue { UnitPrice = _expensivePriceGBP},
                    new PriceValue { UnitPrice = _cheapPriceUSD}
                });

            _subject = new PricingService(
                _priceServiceMock.Object,
                _currentMarketMock.Object,
                _currencyServiceMock.Object,
                new FakeAppContext());
        }

        
    }
}
