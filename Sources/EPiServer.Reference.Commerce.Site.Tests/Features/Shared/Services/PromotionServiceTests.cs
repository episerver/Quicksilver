using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Framework.Cache;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Pricing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Shared.Services
{
    [TestClass]
    public class PromotionServiceTests
    {
        [TestMethod]
        public void GetDiscountPrice_WhenEmptyCurrencyIsProvided_ShouldReturnDiscountedPriceBasedOnMarket()
        {
            var priceWithDiscount = _subject.GetDiscountPrice(
                new CatalogKey(_appContext.ApplicationId, _variation1.Code), _USMarketMock.Object.MarketId, Currency.Empty);

            var cheapestPrice = CreatePriceList(_variation1.Code, Currency.USD).OrderBy(x => x.UnitPrice.Amount).First();
            var expectedUnitPrice = new Money(cheapestPrice.UnitPrice.Amount - _discountAmount,
                cheapestPrice.UnitPrice.Currency);

            Assert.AreEqual<Money>(expectedUnitPrice, priceWithDiscount.UnitPrice);
        }

        [TestMethod]
        public void GetDiscountPrice_WhenNonEmptyCurrencyIsProvided_ShouldReturnDiscountedPriceBasedOnProvidedCurrency()
        {
            var priceWithDiscount = _subject.GetDiscountPrice(
                new CatalogKey(_appContext.ApplicationId, _variation1.Code), _USMarketMock.Object.MarketId, Currency.USD);

            var cheapestPrice = CreatePriceList(_variation1.Code, Currency.USD).OrderBy(x => x.UnitPrice.Amount).First();
            var expectedUnitPrice = new Money(cheapestPrice.UnitPrice.Amount - _discountAmount,
                cheapestPrice.UnitPrice.Currency);

            Assert.AreEqual<Money>(expectedUnitPrice, priceWithDiscount.UnitPrice);
        }

        [TestMethod]
        public void GetDiscountPrice_WhenMismatchBetweenCurrencyAndMarketCurrency_ShouldReturnNoPrice()
        {
            var priceWithDiscount = _subject.GetDiscountPrice(
                new CatalogKey(_appContext.ApplicationId, _variation1.Code), _USMarketMock.Object.MarketId, Currency.SEK);

            Assert.IsNull(priceWithDiscount);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetDiscountPrice_WhenMarketIsUnknown_ShouldThrow()
        {
            var priceWithDiscount = _subject.GetDiscountPrice(
                new CatalogKey(_appContext.ApplicationId, _variation1.Code), new MarketId("UNKNOWN"), Currency.Empty);

            Assert.IsNull(priceWithDiscount);
        }

        [TestMethod]
        public void GetDiscountPrice_WhenNoPricesAvailableForMarket_ShouldReturnNull()
        {
            _pricingServiceMock
                .Setup(x => x.GetPriceList(It.IsAny<string>(), It.IsAny<MarketId>(), It.IsAny<PriceFilter>()))
                .Returns(() => Enumerable.Empty<IPriceValue>().ToList());

            var priceWithDiscount = _subject.GetDiscountPrice(
                new CatalogKey(_appContext.ApplicationId, _variation1.Code), _USMarketMock.Object.MarketId, Currency.USD);

            Assert.IsNull(priceWithDiscount);
        }

        private PromotionService _subject;
        private FakeAppContext _appContext;
        private Mock<IPricingService> _pricingServiceMock;
        private List<VariationContent> _variations;
        private Mock<IMarketService> _marketServiceMock;
        private Mock<IContentLoader> _contentLoaderMock;
        private Mock<IPromotionEntryService> _promotionEntryServiceMock;
        private Mock<ReferenceConverter> _referenceConverterMock;
        private decimal _discountAmount;
        private Mock<IMarket> _USMarketMock;
        private Mock<IMarket> _SEKMarketMock;
        private VariationContent _variation1;
        private VariationContent _variation2;

        [TestInitialize]
        public void Setup()
        {
            _appContext = new FakeAppContext();

            SetupContent();
            SetupPricing();
            SetupMarkets();
            SetupReferenceConverter();
            SetupPromotionEntryService();
            
            _subject = new PromotionService(
                _pricingServiceMock.Object,
                _marketServiceMock.Object,
                _contentLoaderMock.Object,
                _referenceConverterMock.Object,
                new FakePromotionHelper(), 
                _promotionEntryServiceMock.Object);
        }

        private void SetupReferenceConverter()
        {
            var synchronizedObjectInstanceCache = new Mock<ISynchronizedObjectInstanceCache>();

            _referenceConverterMock = new Mock<ReferenceConverter>(
                new EntryIdentityResolver(synchronizedObjectInstanceCache.Object),
                new NodeIdentityResolver(synchronizedObjectInstanceCache.Object))
            {
                CallBase = true
            };
            _referenceConverterMock
                .Setup(x => x.GetContentLink(It.IsAny<string>(), CatalogContentType.CatalogEntry))
                .Returns((string catalogEntryCode, CatalogContentType type) => 
                        _variations.FirstOrDefault(x => x.Code == catalogEntryCode).ContentLink);
        }

        private void SetupPromotionEntryService()
        {
            _promotionEntryServiceMock = new Mock<IPromotionEntryService>();

            _discountAmount = 0.3m;

            var price = CreatePriceList(_variation1.Code, Currency.USD).OrderBy(x => x.UnitPrice.Amount).First();

            _promotionEntryServiceMock
                .Setup(x => x.GetDiscountPrice(
                    It.IsAny<IPriceValue>(),
                    It.IsAny<EntryContentBase>(),
                    It.IsAny<Currency>(),
                    It.IsAny<PromotionHelperFacade>()))
                .Returns((IPriceValue priceValue, EntryContentBase entry, Currency currency, PromotionHelperFacade promotionHelper) =>  new PriceValue
                    {
                        UnitPrice = new Money(price.UnitPrice.Amount - _discountAmount, currency)
                    });
        }

        private void SetupMarkets()
        {
            _USMarketMock = new Mock<IMarket>();
            _USMarketMock.Setup(x => x.MarketId).Returns(new MarketId("US"));
            _USMarketMock.Setup(x => x.DefaultCurrency).Returns(new Currency("USD"));

            _SEKMarketMock = new Mock<IMarket>();
            _SEKMarketMock.Setup(x => x.MarketId).Returns(new MarketId("SE"));
            _SEKMarketMock.Setup(x => x.DefaultCurrency).Returns(new Currency("SEK"));

            var markets = new List<IMarket> {_USMarketMock.Object, _SEKMarketMock.Object};

            _marketServiceMock = new Mock<IMarketService>();
            _marketServiceMock.Setup(x => x.GetMarket(It.IsAny<MarketId>()))
                .Returns((MarketId marketId) => markets.SingleOrDefault(x => x.MarketId == marketId));
        }

        private void SetupContent()
        {
            _variation1 = new VariationContent
            {
                Code = "code1",
                ContentLink = new ContentReference(1),
                Categories = new Categories {ContentLink = new ContentReference(11)}
            };

            _variation2 = new VariationContent
            {
                Code = "code2",
                ContentLink = new ContentReference(2),
                Categories = new Categories { ContentLink = new ContentReference(11) }
            };

            _variations = new List<VariationContent>
            {
                _variation1, _variation2
            };

            _contentLoaderMock = new Mock<IContentLoader>();
            _contentLoaderMock.Setup(x => x.Get<EntryContentBase>(It.IsAny<ContentReference>()))
                .Returns((ContentReference contentReference) => _variations.SingleOrDefault(x => x.ContentLink == contentReference));
        }

        private void SetupPricing()
        {
            _pricingServiceMock = new Mock<IPricingService>();
            _pricingServiceMock
                .Setup(x => x.GetPriceList(It.IsAny<string>(), It.IsAny<MarketId>(), It.IsAny<PriceFilter>()))
                .Returns((string code, MarketId marketId, PriceFilter priceFilter) => CreatePriceList(code, Currency.USD));
        }

        private IList<IPriceValue> CreatePriceList(string code, Currency currency)
        {
            var priceList = new List<IPriceValue>()
            {
                new PriceValue
                {
                    CatalogKey = new CatalogKey(_appContext.ApplicationId, code),
                    UnitPrice = new Money(2,currency)
                },
                new PriceValue
                {
                    CatalogKey = new CatalogKey(_appContext.ApplicationId, code),
                    UnitPrice = new Money(1,currency)
                },
            };

            return priceList;
        }
    }
}
