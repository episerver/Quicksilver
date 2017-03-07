using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.Core;
using EPiServer.Filters;
using EPiServer.Globalization;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Product.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Pricing;
using Moq;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Product.ViewModelFactories
{
    public class CatalogEntryViewModelFactoryTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("something")]
        public void Create_WhenVariationCodeIsNotValid_ShouldReturnModelWithNoVariation(string variationCode)
        {
            var fashionProduct = CreateFashionProduct();
            SetRelation(fashionProduct, Enumerable.Empty<ProductVariation>());
            
            var result = CreateSubject().Create(fashionProduct, variationCode);
            
            Assert.Null(result.Variant);
        }

        [Fact]
        public void Create_WhenSelectedVariationDontExist_ShouldReturnModelWithNoVariation()
        {
            var fashionProduct = CreateFashionProduct();
            var fashionVariant = CreateFashionVariant();
            SetRelation(fashionProduct, fashionVariant);

            var result = CreateSubject().Create(fashionProduct, "doNotExist");

            Assert.Null(result.Variant);
        }

        [Fact]
        public void Create_WhenSelectedVariationExist_ShouldSetVariationToSelectedVariation()
        {
            var fashionProduct = CreateFashionProduct();
            var fashionVariant = CreateFashionVariant();
            SetRelation(fashionProduct, fashionVariant);

            var result = CreateSubject().Create(fashionProduct, fashionVariant.Code);

            Assert.Equal<FashionVariant>(fashionVariant, result.Variant);
        }

        [Fact]
        public void Create_WhenSelectedVariationExist_ShouldSetProductToProduct()
        {
            var fashionProduct = CreateFashionProduct();
            var fashionVariant = CreateFashionVariant();
            SetRelation(fashionProduct, fashionVariant);
            MockPrices();
            
            var result = CreateSubject().Create(fashionProduct, fashionVariant.Code);

            Assert.Equal<FashionProduct>(fashionProduct, result.Product);
        }

        [Fact]
        public void Create_WhenSelectedVariationExist_ShouldSetOriginalPriceToDefaultPrice()
        {
            var fashionProduct = CreateFashionProduct();
            var fashionVariant = CreateFashionVariant();
            SetRelation(fashionProduct, fashionVariant);

            var mockDefaultPrice = CreatePriceValueMock(25);
            SetDefaultPriceService(mockDefaultPrice.Object);

            var mockDiscountPrice = CreatePriceValueMock(20);
            SetDiscountPriceService(mockDiscountPrice.Object);

            var result = CreateSubject().Create(fashionProduct, fashionVariant.Code);

            Assert.Equal<Money>(mockDefaultPrice.Object.UnitPrice, result.ListingPrice);
        }

        [Fact]
        public void Create_WhenSelectedVariationExist_ShouldSetPriceToDiscountPrice()
        {
            var fashionProduct = CreateFashionProduct();
            var fashionVariant = CreateFashionVariant();
            SetRelation(fashionProduct, fashionVariant);

            var mockDefaultPrice = CreatePriceValueMock(25);
            SetDefaultPriceService(mockDefaultPrice.Object);

            var mockDiscountPrice = CreatePriceValueMock(20);
            SetDiscountPriceService(mockDiscountPrice.Object);

            var result = CreateSubject().Create(fashionProduct, fashionVariant.Code);

            Assert.Equal<Money>(mockDiscountPrice.Object.UnitPrice, result.DiscountedPrice.Value);
        }

        [Fact]
        public void Create_WhenSelectedVariationExist_ShouldSetColorToSelectedVariationColor()
        {
            var color = "green";
            var fashionProduct = CreateFashionProduct();
            var fashionVariant = CreateFashionVariant();
            fashionVariant.Color = color;

            SetRelation(fashionProduct, fashionVariant);
            MockPrices();

            var result = CreateSubject().Create(fashionProduct, fashionVariant.Code);

            Assert.Equal<string>(color, result.Color);
        }

        [Fact]
        public void Create_WhenSelectedVariationExist_ShouldSetSizeToSelectedVariationSize()
        {
            var size = "small";
            var fashionProduct = CreateFashionProduct();
            var fashionVariant = CreateFashionVariant();
            fashionVariant.Size = size;

            SetRelation(fashionProduct, fashionVariant);
            MockPrices();

            var result = CreateSubject().Create(fashionProduct, fashionVariant.Code);

            Assert.Equal<string>(size, result.Size);
        }

        [Fact]
        public void Create_WhenSelectedVariationDontHaveAssets_ShouldSetImagesToOneItemWithEmptyLink()
        {
            var fashionProduct = CreateFashionProduct();
            var fashionVariant = CreateFashionVariant();

            SetRelation(fashionProduct, fashionVariant);
            MockPrices();

            var result = CreateSubject().Create(fashionProduct, fashionVariant.Code);

            Assert.Equal<string>(string.Empty, result.Images.Single());
        }

        [Fact]
        public void Create_WhenSelectedVariationHasImageAssets_ShouldSetImagesToLinkFromImage()
        {
            var imageLink = "http://www.episerver.com";

            var fashionProduct = CreateFashionProduct();
            var fashionVariant = CreateFashionVariant();

            IContentImage contentImage;

            var imageMedia = new CommerceMedia {AssetLink = new ContentReference(237)};
            _contentLoaderMock.Setup(x => x.TryGet(imageMedia.AssetLink, out contentImage)).Returns(true);
            _urlResolverMock.Setup(x => x.GetUrl(imageMedia.AssetLink)).Returns(imageLink);

            fashionVariant.CommerceMediaCollection = new ItemCollection<CommerceMedia>() {imageMedia};
            SetRelation(fashionProduct, fashionVariant);
            MockPrices();

            var result = CreateSubject().Create(fashionProduct, fashionVariant.Code);

            Assert.Equal<string>(imageLink, result.Images.Single());
        }

        [Fact]
        public void Create_WhenAvailableColorsAreEmptyForVariation_ShouldSetColorsToEmpty()
        {
            var fashionProduct = CreateFashionProduct();
            var fashionVariant = CreateFashionVariant();
            SetRelation(fashionProduct, fashionVariant);
            MockPrices();

            var result = CreateSubject().Create(fashionProduct, fashionVariant.Code);

            Assert.Equal<int>(0, result.Colors.Count());
        }

        [Fact]
        public void Create_WhenAvailableColorsContainsItems_ShouldSetTextToItemValue()
        {
            var colors = new ItemCollection<string>() {"green", "red"};
             
            var fashionProduct = CreateFashionProduct();
            fashionProduct.AvailableColors = colors;
            fashionProduct.AvailableSizes = new ItemCollection<string> {"small", "medium"};
             
            var fashionVariant1 = CreateFashionVariant("small", "green");
            var fashionVariant2 = CreateFashionVariant("medium", "red");
            var fashionVariant3 = CreateFashionVariant("medium", "green");

            SetRelation(fashionProduct, new[] {fashionVariant1, fashionVariant2, fashionVariant3});
            MockPrices();

            var result = CreateSubject().Create(fashionProduct, fashionVariant1.Code);

            var expectedColors = String.Join(";", colors);
            var modelColorTexts = String.Join(";", result.Colors.Select(x => x.Text));

            Assert.Equal<string>(expectedColors, modelColorTexts);
        }

        [Fact]
        public void Create_WhenAvailableColorsContainsItems_ShouldSetValueToItemValue()
        {
            var colors = new ItemCollection<string>() {"green", "red"};

            var fashionProduct = CreateFashionProduct();
            fashionProduct.AvailableColors = colors;
            fashionProduct.AvailableSizes = new ItemCollection<string> {"small", "medium"};

            var fashionVariant1 = CreateFashionVariant("small", "green");
            var fashionVariant2 = CreateFashionVariant("medium", "red");
            var fashionVariant3 = CreateFashionVariant("medium", "green");

            SetRelation(fashionProduct, new[] {fashionVariant1, fashionVariant2, fashionVariant3});
            MockPrices();

            var result = CreateSubject().Create(fashionProduct, fashionVariant1.Code);

            var expectedColors = String.Join(";", colors);
            var modelColorValues = String.Join(";", result.Colors.Select(x => x.Value));

            Assert.Equal<string>(expectedColors, modelColorValues);
        }

        [Fact]
        public void Create_WhenAvailableColorsContainsItems_ShouldSetSelectedToFalse()
        {
            var fashionProduct = CreateFashionProduct();
            fashionProduct.AvailableColors = new ItemCollection<string>() {"green", "red"};
            fashionProduct.AvailableSizes = new ItemCollection<string> {"small", "medium"};

            var fashionVariant1 = CreateFashionVariant("small", "green");
            var fashionVariant2 = CreateFashionVariant("medium", "red");
            var fashionVariant3 = CreateFashionVariant("medium", "green");

            SetRelation(fashionProduct, new[] {fashionVariant1, fashionVariant2, fashionVariant3});
            MockPrices();

            var result = CreateSubject().Create(fashionProduct, fashionVariant1.Code);

            var expectedColors = String.Join(";", new[] {false, false});
            var modelColorsSelected = String.Join(";", result.Colors.Select(x => x.Selected));

            Assert.Equal<string>(expectedColors, modelColorsSelected);
        }

        [Fact]
        public void Create_WhenAvailableSizesContainsItems_ShouldSetTextToItemValue()
        {
            var sizes = new ItemCollection<string>() {"medium"};

            var fashionProduct = CreateFashionProduct();
            var fashionVariant = CreateFashionVariant("medium", "red");
            SetRelation(fashionProduct, fashionVariant);
            MockPrices();

            var result = CreateSubject().Create(fashionProduct, fashionVariant.Code);

            var expectedSizes = String.Join(";", sizes);
            var modelSizeTexts = String.Join(";", result.Sizes.Select(x => x.Text));

            Assert.Equal<string>(expectedSizes, modelSizeTexts);
        }

        [Fact]
        public void Create_WhenAvailableSizesContainsItems_ShouldSetValueToItemValue()
        {
            var sizes = new ItemCollection<string>() {"medium"};

            var fashionProduct = CreateFashionProduct();
            var fashionVariant = CreateFashionVariant("medium", "red");
            SetRelation(fashionProduct, fashionVariant);
            MockPrices();

            var result = CreateSubject().Create(fashionProduct, fashionVariant.Code);

            var expectedSizes = String.Join(";", sizes);
            var modelSizeValues = String.Join(";", result.Sizes.Select(x => x.Value));

            Assert.Equal<string>(expectedSizes, modelSizeValues);
        }

        [Fact]
        public void Create_WhenAvailableSizesContainsItems_ShouldSetSelectedToFalse()
        {
            var fashionProduct = CreateFashionProduct();
            var fashionVariant = CreateFashionVariant("medium", "red");
            SetRelation(fashionProduct, fashionVariant);
            MockPrices();

            var result = CreateSubject().Create(fashionProduct, fashionVariant.Code);

            var expectedSizes = String.Join(";", new[] {false});
            var modelSizesSelected = String.Join(";", result.Sizes.Select(x => x.Selected));

            Assert.Equal<string>(expectedSizes, modelSizesSelected);
        }

        [Fact]
        public void Create_WhenVariationCodeHasValue_ShouldSetColorsToTheAvailableColorsForTheVariationSize()
        {
            const string variationColorBlue = "blue";
            const string variationColorWhite = "white";
           
            var sizes = new ItemCollection<string>() {"small", "medium"};
            var colors = new ItemCollection<string>()
            {
                "red",
                variationColorBlue,
                "yellow",
                variationColorWhite,
                "green"
            };

            var fashionProduct = CreateFashionProduct();
            fashionProduct.AvailableSizes = sizes;

            fashionProduct.AvailableColors = colors;

            var fashionVariantSmallBlue = CreateFashionVariant("small", variationColorBlue);
            var fashionVariantSmallWhite = CreateFashionVariant("small", variationColorWhite);

            SetRelation(fashionProduct, new[] {fashionVariantSmallBlue, fashionVariantSmallWhite});
            MockPrices();

            var result = CreateSubject().Create(fashionProduct, fashionVariantSmallBlue.Code);

            var expectedColors = String.Join(";", new[] {variationColorBlue, variationColorWhite});
            var modelColors = String.Join(";", result.Colors.Select(x => x.Value));

            Assert.Equal<string>(expectedColors, modelColors);
        }

        [Fact]
        public void Create_WhenVariationCodeHasValue_ShouldSetSizesToTheAvailableSizesForTheVariationColor()
        {
            const string variationSizeMedium = "medium";
            const string variationSizeXlarge = "x-large";
           
            var sizes = new ItemCollection<string>()
            {
                "small",
                variationSizeMedium,
                "large",
                variationSizeXlarge,
                "xx-large"
            };
            var colors = new ItemCollection<string>() {"red"};

            var fashionProduct = CreateFashionProduct();
            fashionProduct.AvailableSizes = sizes;
            fashionProduct.AvailableColors = colors;

            var fashionVariantMediumRed = CreateFashionVariant(variationSizeMedium, "red");
            var fashionVariantXlargeRed = CreateFashionVariant(variationSizeXlarge, "red");

            SetRelation(fashionProduct, new[] {fashionVariantMediumRed, fashionVariantXlargeRed});
            MockPrices();

            var result = CreateSubject().Create(fashionProduct, fashionVariantMediumRed.Code);

            var expectedSizes = String.Join(";", new[] {variationSizeMedium, variationSizeXlarge});
            var modelSizes = String.Join(";", result.Sizes.Select(x => x.Value));

            Assert.Equal<string>(expectedSizes, modelSizes);
        }

        [Fact]
        public void Create_WhenAvailableSizesContainsDelayPublishItems_ShouldReturnModelWithNoVariant()
        {
            var sizes = new ItemCollection<string>() {"small", "medium"};

            var fashionProduct = CreateFashionProduct();
            fashionProduct.AvailableSizes = sizes;

            var fashionVariant = CreateFashionVariant();
            fashionVariant.StartPublish = DateTime.UtcNow.AddDays(7); // pulish date is future
            fashionVariant.StopPublish = DateTime.UtcNow.AddDays(17);
            fashionVariant.Status = VersionStatus.DelayedPublish;

            SetRelation(fashionProduct, fashionVariant);
            MockPrices();

            var result = CreateSubject().Create(fashionProduct, fashionVariant.Code);

            Assert.Null(result.Variant);
        }

        [Fact]
        public void Create_WhenAvailableSizesContainsExpiredItems_ShouldReturnModelWithNoVariant()
        {
            var sizes = new ItemCollection<string>() {"small", "medium"};

            var fashionProduct = CreateFashionProduct();
            fashionProduct.AvailableSizes = sizes;

            var fashionVariant = CreateFashionVariant();
            fashionVariant.StartPublish = DateTime.UtcNow.AddDays(-17);
            fashionVariant.StopPublish = DateTime.UtcNow.AddDays(-7);

            SetRelation(fashionProduct, fashionVariant);
            MockPrices();

            var result = CreateSubject().Create(fashionProduct, fashionVariant.Code);

            Assert.Null(result.Variant);
        }

        [Fact]
        public void Create_WhenAvailableSizesContainsUnpublishItems_ShouldReturnModelWithNoVariant()
        {
            var sizes = new ItemCollection<string>() {"small", "medium"};

            var fashionProduct = CreateFashionProduct();
            fashionProduct.AvailableSizes = sizes;

            var fashionVariant = CreateFashionVariant();
            fashionVariant.IsPendingPublish = true;
            fashionVariant.Status = VersionStatus.CheckedIn;

            SetRelation(fashionProduct, fashionVariant);
            MockPrices();

            var result = CreateSubject().Create(fashionProduct, fashionVariant.Code);

            Assert.Null(result.Variant);
        }

        [Fact]
        public void Create_WhenAvailableSizesContainsItemUnavailabelInCurrentMarket_ShouldReturnModelWithNoVariant()
        {
            var sizes = new ItemCollection<string>() {"small", "medium"};

            var fashionProduct = CreateFashionProduct();
            fashionProduct.AvailableSizes = sizes;

            // setup variant unavailable in default market
            var fashionVariant = CreateFashionVariant();
            fashionVariant.MarketFilter = new ItemCollection<string>() {"Default"};

            SetRelation(fashionProduct, fashionVariant);
            MockPrices();

            var result = CreateSubject().Create(fashionProduct, fashionVariant.Code);

            Assert.Null(result.Variant);
        }
        
        [Fact]
        public void SelectVariant_WhenColorAndSizeHasValues_ShouldGetVariantWithSelectedColorAndSize()
        {
            var sizes = new ItemCollection<string>() {"small", "medium"};
            var colors = new ItemCollection<string>() {"green", "red"};

            var fashionProduct = CreateFashionProduct();
            fashionProduct.AvailableSizes = sizes;

            fashionProduct.AvailableColors = colors;

            var fashionVariantSmallGreen = CreateFashionVariant("small", "green");
            var fashionVariantSmallRed = CreateFashionVariant("small", "red");
            var fashionVariantMediumGreen = CreateFashionVariant("medium", "green");
            var fashionVariantMediumRed = CreateFashionVariant("medium", "red");

            SetRelation(fashionProduct, new[]
            {
                fashionVariantSmallGreen,
                fashionVariantSmallRed,
                fashionVariantMediumGreen,
                fashionVariantMediumRed,
            });

            var result = CreateSubject().SelectVariant(fashionProduct, "red", "small");

            Assert.Equal<string>("redsmall", result.Code);
        }

        [Fact]
        public void SelectVariant_WhenCanNotFoundBySize_ShouldTryGetVariantWithSelectedColorOnly()
        {
            var sizes = new ItemCollection<string>() {"small", "medium"};
            var colors = new ItemCollection<string>() {"green", "red"};

            var fashionProduct = CreateFashionProduct();
            fashionProduct.AvailableSizes = sizes;

            fashionProduct.AvailableColors = colors;

            var fashionVariantSmallGreen = CreateFashionVariant("small", "green");
            var fashionVariantMediumRed = CreateFashionVariant("medium", "red");

            SetRelation(fashionProduct, new[]
            {
                fashionVariantSmallGreen,
                fashionVariantMediumRed,
            });

            var result = CreateSubject().SelectVariant(fashionProduct, "red", "small");

            Assert.Equal<string>("redmedium", result.Code);
        }

        [Fact]
        public void SelectVariant_WhenCanNotFoundBySizeOrColor_ShouldReturnHttpNotFoundResult()
        {
            var sizes = new ItemCollection<string>() {"small", "medium"};
            var colors = new ItemCollection<string>() {"green", "red"};

            var fashionProduct = CreateFashionProduct();
            fashionProduct.AvailableSizes = sizes;

            fashionProduct.AvailableColors = colors;

            var fashionVariantSmallGreen = CreateFashionVariant("small", "green");
            var fashionVariantMediumRed = CreateFashionVariant("medium", "red");

            SetRelation(fashionProduct, new[]
            {
                fashionVariantSmallGreen,
                fashionVariantMediumRed,
            });

            var result = CreateSubject().SelectVariant(fashionProduct, "yellow", "small");

            Assert.Null(result);
        }

        private readonly Mock<IPromotionService> _promotionServiceMock;
        private readonly Mock<IContentLoader> _contentLoaderMock;
        private readonly Mock<IPriceService> _priceServiceMock;
        private readonly Mock<ICurrentMarket> _currentMarketMock;
        private readonly FilterPublished _filterPublished;
        private readonly Mock<CurrencyService> _currencyserviceMock;
        private readonly Mock<IRelationRepository> _relationRepositoryMock;
        private readonly Mock<CookieService> _cookieServiceMock;
        private readonly Mock<UrlResolver> _urlResolverMock;
        private readonly Mock<HttpContextBase> _httpContextBaseMock;
        private readonly Mock<IMarket> _marketMock;
        private readonly Mock<AppContextFacade> _appContextFacadeMock;
        private readonly Mock<LanguageResolver> _languageResolverMock;
        private readonly Currency _defaultCurrency;

        public CatalogEntryViewModelFactoryTests()
        {
            _defaultCurrency = Currency.USD;

            _urlResolverMock = new Mock<UrlResolver>();
            _contentLoaderMock = new Mock<IContentLoader>();
            _cookieServiceMock = new Mock<CookieService>();
            _priceServiceMock = new Mock<IPriceService>();
            _relationRepositoryMock = new Mock<IRelationRepository>();
            _promotionServiceMock = new Mock<IPromotionService>();

            var mockPublishedStateAssessor = new Mock<IPublishedStateAssessor>();
            mockPublishedStateAssessor.Setup(x => x.IsPublished(It.IsAny<IContent>(), It.IsAny<PublishedStateCondition>()))
                .Returns((IContent content, PublishedStateCondition condition) =>
                {
                    var contentVersionable = content as IVersionable;
                    if (contentVersionable != null)
                    {
                        if (contentVersionable.Status == VersionStatus.Published &&
                            contentVersionable.StartPublish < DateTime.UtcNow &&
                            contentVersionable.StopPublish > DateTime.UtcNow)
                        {
                            return true;
                        }
                    }
                    return false;
                }
                );

            _filterPublished = new FilterPublished(mockPublishedStateAssessor.Object);

            _marketMock = new Mock<IMarket>();
            _marketMock.Setup(x => x.DefaultCurrency).Returns(_defaultCurrency);
            _marketMock.Setup(x => x.MarketId).Returns(new MarketId("Default"));
            _marketMock.Setup(x => x.MarketName).Returns("Default");
            _marketMock.Setup(x => x.IsEnabled).Returns(true);
            _marketMock.Setup(x => x.DefaultLanguage).Returns(new CultureInfo("en"));

            _currentMarketMock = new Mock<ICurrentMarket>();
            _currentMarketMock.Setup(x => x.GetCurrentMarket()).Returns(_marketMock.Object);

            _currencyserviceMock = new Mock<CurrencyService>(_currentMarketMock.Object, _cookieServiceMock.Object);
            _currencyserviceMock.Setup(x => x.GetCurrentCurrency()).Returns(_defaultCurrency);

            _appContextFacadeMock = new Mock<AppContextFacade>();
            _appContextFacadeMock.Setup(x => x.ApplicationId).Returns(Guid.NewGuid);

            var request = new Mock<HttpRequestBase>();
            request.SetupGet(x => x.Headers).Returns(
                new System.Net.WebHeaderCollection {
                {"X-Requested-With", "XMLHttpRequest"}
            });

            _httpContextBaseMock = new Mock<HttpContextBase>();
            _httpContextBaseMock.SetupGet(x => x.Request).Returns(request.Object);

            _languageResolverMock = new Mock<LanguageResolver>();
            _languageResolverMock.Setup(x => x.GetPreferredCulture()).Returns(CultureInfo.GetCultureInfo("en"));

            SetGetItems(Enumerable.Empty<ContentReference>(), Enumerable.Empty<IContent>());
            _cookieServiceMock.Setup(x => x.Get("Currency")).Returns((string)null);
        }
     
        private CatalogEntryViewModelFactory CreateSubject()
        {
            return new CatalogEntryViewModelFactory(
               _promotionServiceMock.Object,
               _contentLoaderMock.Object,
               _priceServiceMock.Object,
               _currentMarketMock.Object,
               _currencyserviceMock.Object,
               _relationRepositoryMock.Object,
               _appContextFacadeMock.Object,
               _urlResolverMock.Object,
               _filterPublished,
               _languageResolverMock.Object);
        }

        private static FashionVariant CreateFashionVariant(string size, string color)
        {
            var fashionVariant = CreateFashionVariant(color + size);
            fashionVariant.Size = size;
            fashionVariant.Color = color;

            return fashionVariant;
        }

        private static FashionVariant CreateFashionVariant(string code = "myVariant")
        {
            return new FashionVariant
            {
                ContentLink = new ContentReference(740),
                Code = code,
                IsDeleted = false,
                IsPendingPublish = false,
                Status = VersionStatus.Published,
                StartPublish = DateTime.UtcNow.AddDays(-7),
                StopPublish = DateTime.UtcNow.AddDays(7),
                MarketFilter = new ItemCollection<string>() { "USA" }
            };
        }

        private static FashionProduct CreateFashionProduct()
        {
            return new FashionProduct
            {
                ContentLink = new ContentReference(741),
                Code = "myProduct",
                IsDeleted = false,
                IsPendingPublish = false,
                Status = VersionStatus.Published,
                StartPublish = DateTime.UtcNow.AddDays(-7),
                StopPublish = DateTime.UtcNow.AddDays(7),
                MarketFilter = new ItemCollection<string>() { "USA" },
                AvailableColors = new ItemCollection<string>(),
                AvailableSizes = new ItemCollection<string>()
            };
        }

        private void SetRelation(IContent source, IContent target)
        {
            SetRelation(source, new[] { target });
        }

        private void SetRelation(IContent source, IEnumerable<IContent> targets)
        {
            SetRelation(source, targets.Select(x => new ProductVariation() { Source = source.ContentLink, Target = x.ContentLink }));

            SetGetItems(new[] { source.ContentLink }, new[] { source });
            SetGetItems(targets.Select(x => x.ContentLink), targets);
        }

        private void SetRelation(IContent setup, IEnumerable<ProductVariation> result)
        {
            _relationRepositoryMock.Setup(x => x.GetRelationsBySource<ProductVariation>(setup.ContentLink)).Returns(result);
        }

        private void SetGetItems(IEnumerable<ContentReference> setup, IEnumerable<IContent> result)
        {
            _contentLoaderMock.Setup(x => x.GetItems(setup, CultureInfo.GetCultureInfo("en"))).Returns(result);
        }

        private void SetDefaultPriceService(IPriceValue returnedPrice)
        {
            _priceServiceMock
                .Setup(x => x.GetDefaultPrice(It.IsAny<MarketId>(), It.IsAny<DateTime>(), It.IsAny<CatalogKey>(), _defaultCurrency))
                .Returns(returnedPrice);
        }

        private void SetDiscountPriceService(IPriceValue returnedPrice)
        {
            _promotionServiceMock
                .Setup(x => x.GetDiscountPrice(It.IsAny<CatalogKey>(), It.IsAny<MarketId>(), _defaultCurrency))
                .Returns(returnedPrice);
        }

        private Mock<IPriceValue> CreatePriceValueMock(decimal amount)
        {
            var mockPriceValue = new Mock<IPriceValue>();
            mockPriceValue.Setup(x => x.ValidFrom).Returns(DateTime.MinValue);
            mockPriceValue.Setup(x => x.ValidUntil).Returns(DateTime.MaxValue);
            mockPriceValue.Setup(x => x.UnitPrice).Returns(new Money(amount, _defaultCurrency));

            return mockPriceValue;
        }

        private void MockPrices()
        {
            var mockDefaultPrice = CreatePriceValueMock(25);
            SetDefaultPriceService(mockDefaultPrice.Object);

            var mockDiscountPrice = CreatePriceValueMock(20);
            SetDiscountPriceService(mockDiscountPrice.Object);
        }
    }
}
