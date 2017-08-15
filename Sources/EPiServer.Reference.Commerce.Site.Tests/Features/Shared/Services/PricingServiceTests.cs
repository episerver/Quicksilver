using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Pricing;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Shared.Services
{
    public class PricingServiceTests
    {
        [Fact]
        public void GetPrice_WhenPricesExist_ShouldReturnCheapestPrice()
        {
            var price = _subject.GetPrice("code", new MarketId(), Currency.USD);

            Assert.Equal<Money>(new Money(1, "USD"), price.UnitPrice);
        }

        [Fact]
        public void GetPrice_WhenPricesDoNotExist_ShouldReturnNull()
        {
            _priceServiceMock
                .Setup(x => x.GetPrices(It.IsAny<MarketId>(),It.IsAny<DateTime>(),It.IsAny<CatalogKey>(),It.IsAny<PriceFilter>()))
                .Returns(Enumerable.Empty<IPriceValue>);

            var price = _subject.GetPrice("code");

            Assert.Null(price);
        }

        [Fact]
        public void GetDiscountPrice_WhenEmptyCurrencyIsProvided_ShouldReturnDiscountedPriceBasedOnMarket()
        {
            var priceWithDiscount = _subject.GetDiscountPrice(
                new CatalogKey("code1"), new MarketId("US"), Currency.Empty);
            
            var expectedUnitPrice = new Money(1 - 0.3m, Currency.USD);

            Assert.Equal<Money>(expectedUnitPrice, priceWithDiscount.UnitPrice);
        }

        [Fact]
        public void GetDiscountPrice_WhenNonEmptyCurrencyIsProvided_ShouldReturnDiscountedPriceBasedOnProvidedCurrency()
        {
            var priceWithDiscount = _subject.GetDiscountPrice(
                new CatalogKey("code1"), new MarketId("US"), Currency.USD);

            var expectedUnitPrice = new Money(1 - 0.3m, Currency.USD);

            Assert.Equal<Money>(expectedUnitPrice, priceWithDiscount.UnitPrice);
        }

        [Fact]
        public void GetDiscountPrice_WhenMismatchBetweenCurrencyAndMarketCurrency_ShouldReturnNoPrice()
        {
            var priceWithDiscount = _subject.GetDiscountPrice(
                new CatalogKey("code1"), new MarketId("US"), Currency.SEK);

            Assert.Null(priceWithDiscount);
        }

        [Fact]
        public void GetDiscountPrice_WhenNoPricesAvailableForMarket_ShouldReturnNull()
        {
            _priceServiceMock
                .Setup(x => x.GetPrices(It.IsAny<MarketId>(), It.IsAny<DateTime>(), It.IsAny<CatalogKey>(), It.IsAny<PriceFilter>()))
                .Returns(() => Enumerable.Empty<IPriceValue>().ToList());

            var priceWithDiscount = _subject.GetDiscountPrice(
                new CatalogKey("code1"), new MarketId("US"), Currency.USD);

            Assert.Null(priceWithDiscount);
        }

        private readonly PricingServiceForTest _subject;
        private readonly Mock<IPriceService> _priceServiceMock;

        public PricingServiceTests()
        {
            _priceServiceMock = new Mock<IPriceService>();
            _priceServiceMock
                .Setup(x => x.GetPrices(It.IsAny<MarketId>(), It.IsAny<DateTime>(), It.IsAny<CatalogKey>(), It.IsAny<PriceFilter>()))
                .Returns((MarketId marketId, DateTime dateTime, CatalogKey catalogKey, PriceFilter priceFilter) =>
                    CreatePriceList(catalogKey.CatalogEntryCode, Currency.USD));

            var usMarketMock = new Mock<IMarket>();
            usMarketMock.Setup(x => x.MarketId).Returns(new MarketId("US"));
            usMarketMock.Setup(x => x.DefaultCurrency).Returns(new Currency("USD"));

            var marketServiceMock = new Mock<IMarketService>();
            marketServiceMock.Setup(x => x.GetMarket(It.IsAny<MarketId>()))
                .Returns((MarketId marketId) => new[] { usMarketMock.Object }.SingleOrDefault(x => x.MarketId == marketId));

            var currentMarketMock = new Mock<ICurrentMarket>();
            currentMarketMock.Setup(x => x.GetCurrentMarket()).Returns(new Mock<IMarket>().Object);

            var currencyServiceMock = new Mock<ICurrencyService>();
            currencyServiceMock.Setup(x => x.GetCurrentCurrency()).Returns(new Currency("USD"));

            _subject = new PricingServiceForTest(
                _priceServiceMock.Object,
                currentMarketMock.Object,
                currencyServiceMock.Object,
                null,
                null,
                marketServiceMock.Object,
                null,
                null);
        }

        private IEnumerable<IPriceValue> CreatePriceList(string code, Currency currency)
        {
            return new []
            {
                new PriceValue
                {
                    CatalogKey = new CatalogKey(code),
                    UnitPrice = new Money(2,currency)
                },
                new PriceValue
                {
                    CatalogKey = new CatalogKey(code),
                    UnitPrice = new Money(1,currency)
                },
            };
        }

        class PricingServiceForTest : PricingService
        {
            public PricingServiceForTest(IPriceService priceService, ICurrentMarket currentMarket, ICurrencyService currencyService, CatalogContentService catalogContentService, ReferenceConverter referenceConverter, IMarketService marketService, ILineItemCalculator lineItemCalculator, IPromotionEngine promotionEngine) 
                : base(priceService, currentMarket, currencyService, catalogContentService, referenceConverter, marketService, lineItemCalculator, promotionEngine)
            {
            }

            protected override IEnumerable<DiscountedEntry> GetDiscountedPrices(ContentReference contentLink, IMarket market, Currency currency)
            {
                return new []
                {
                    new DiscountedEntry( new ContentReference(1),
                        new [] 
                        {
                            new DiscountPrice(null, new Money(0.7m, Currency.USD), new Money(1, Currency.USD))
                        })
                };
            }

            protected override IEnumerable<EntryContentBase> GetEntries(IEnumerable<IPriceValue> prices)
            {
                return new[] {new VariationContent {Code = "code1"}, new VariationContent { Code = "code2" } };
            }
        }
    }
}
