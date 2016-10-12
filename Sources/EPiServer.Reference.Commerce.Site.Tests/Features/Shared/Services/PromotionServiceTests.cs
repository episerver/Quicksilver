using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Framework.Cache;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes;
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
using EPiServer.Commerce.Order.Internal;
using Xunit;


namespace EPiServer.Reference.Commerce.Site.Tests.Features.Shared.Services
{
    public class PromotionServiceTests
    {
        [Fact]
        public void GetDiscountPrice_WhenEmptyCurrencyIsProvided_ShouldReturnDiscountedPriceBasedOnMarket()
        {
            var priceWithDiscount = _subject.GetDiscountPrice(
                new CatalogKey(_appContext.ApplicationId, _variation1.Code), _USMarketMock.Object.MarketId, Currency.Empty);

            var cheapestPrice = CreatePriceList(_variation1.Code, Currency.USD).OrderBy(x => x.UnitPrice.Amount).First();
            var expectedUnitPrice = new Money(cheapestPrice.UnitPrice.Amount - _discountAmount,
                cheapestPrice.UnitPrice.Currency);

            Assert.Equal<Money>(expectedUnitPrice, priceWithDiscount.UnitPrice);
        }

        [Fact]
        public void GetDiscountPrice_WhenNonEmptyCurrencyIsProvided_ShouldReturnDiscountedPriceBasedOnProvidedCurrency()
        {
            var priceWithDiscount = _subject.GetDiscountPrice(
                new CatalogKey(_appContext.ApplicationId, _variation1.Code), _USMarketMock.Object.MarketId, Currency.USD);

            var cheapestPrice = CreatePriceList(_variation1.Code, Currency.USD).OrderBy(x => x.UnitPrice.Amount).First();
            var expectedUnitPrice = new Money(cheapestPrice.UnitPrice.Amount - _discountAmount,
                cheapestPrice.UnitPrice.Currency);

            Assert.Equal<Money>(expectedUnitPrice, priceWithDiscount.UnitPrice);
        }

        [Fact]
        public void GetDiscountPrice_WhenMismatchBetweenCurrencyAndMarketCurrency_ShouldReturnNoPrice()
        {
            var priceWithDiscount = _subject.GetDiscountPrice(
                new CatalogKey(_appContext.ApplicationId, _variation1.Code), _USMarketMock.Object.MarketId, Currency.SEK);

            Assert.Null(priceWithDiscount);
        }
        
        [Fact]
        public void GetDiscountPrice_WhenMarketIsUnknown_ShouldThrow()
        {
            Assert.Throws<ArgumentException>(() => _subject.GetDiscountPrice(
                new CatalogKey(_appContext.ApplicationId, _variation1.Code), new MarketId("UNKNOWN"), Currency.Empty));
        }

        [Fact]
        public void GetDiscountPrice_WhenNoPricesAvailableForMarket_ShouldReturnNull()
        {
            _pricingServiceMock
                .Setup(x => x.GetPriceList(It.IsAny<string>(), It.IsAny<MarketId>(), It.IsAny<PriceFilter>()))
                .Returns(() => Enumerable.Empty<IPriceValue>().ToList());

            var priceWithDiscount = _subject.GetDiscountPrice(
                new CatalogKey(_appContext.ApplicationId, _variation1.Code), _USMarketMock.Object.MarketId, Currency.USD);

            Assert.Null(priceWithDiscount);
        }

        private PromotionService _subject;
        private FakeAppContext _appContext;
        private Mock<IPricingService> _pricingServiceMock;
        private List<VariationContent> _variations;
        private Mock<IMarketService> _marketServiceMock;
        private Mock<IContentLoader> _contentLoaderMock;
        private Mock<ReferenceConverter> _referenceConverterMock;
        private decimal _discountAmount;
        private Mock<IMarket> _USMarketMock;
        private Mock<IMarket> _SEKMarketMock;
        private VariationContent _variation1;
        private VariationContent _variation2;
        private Mock<ILineItemCalculator> _lineItemcalculatorMock;
        private Mock<IPromotionEngine> _promotionEngineMock;


        public PromotionServiceTests()
        {
            _appContext = new FakeAppContext();

            SetupContent();
            SetupPricing();
            SetupMarkets();
            SetupReferenceConverter();
            SetupPromotionEngine();
            _lineItemcalculatorMock = new Mock<ILineItemCalculator>();
            SetupDiscountedPrice();
            
            _subject = new PromotionService(
                _pricingServiceMock.Object,
                _marketServiceMock.Object,
                _contentLoaderMock.Object,
                _referenceConverterMock.Object,
                _lineItemcalculatorMock.Object,
                _promotionEngineMock.Object
                );
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
            _referenceConverterMock
               .Setup(x => x.GetContentLink(It.IsAny<string>()))
               .Returns((string catalogEntryCode) =>
                       _variations.FirstOrDefault(x => x.Code == catalogEntryCode).ContentLink);
            _referenceConverterMock
                .Setup(x => x.GetCode(It.IsAny<ContentReference>()))
                .Returns((ContentReference catalogContentReference) =>
                       _variations.FirstOrDefault(x => x.ContentLink == catalogContentReference).Code);
         }

        private void SetupPromotionEngine()
        {
            _promotionEngineMock = new Mock<IPromotionEngine>();
            

            _discountAmount = 0.3m;
            var lineItem = new InMemoryLineItem
            {
                Code = "code1",
                Quantity = 1, 
                LineItemDiscountAmount = _discountAmount
            };
            var affectedItems = new[] {lineItem};
            var redemptionDescription = new FakeRedemptionDescription(affectedItems.Select(item => new AffectedEntries(new List<PriceEntry> { new PriceEntry(item) })));

            var rewardDescription = RewardDescription.CreateMoneyReward(FulfillmentStatus.Fulfilled, new [] {redemptionDescription}, null, 0m, null);
            _promotionEngineMock
                .Setup(x => x.Evaluate(
                    It.IsAny<IEnumerable<ContentReference>>(),
                    It.IsAny<IMarket>(),
                    It.IsAny<Currency>(),
                    RequestFulfillmentStatus.Fulfilled
                    ))
                .Returns(new RewardDescription[] { rewardDescription  });
        }

        private void SetupDiscountedPrice()
        {
            var price = CreatePriceList(_variation1.Code, Currency.USD).OrderBy(x => x.UnitPrice.Amount).First();
            _lineItemcalculatorMock.Setup(x => x.GetExtendedPrice(It.IsAny<ILineItem>(), It.IsAny<Currency>()))
               .Returns((ILineItem item, Currency currency) => new Money(price.UnitPrice.Amount - _discountAmount, currency));
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
