using System;
using System.Linq;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Product.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Web.Routing;
using FluentAssertions;
using Mediachase.Commerce;
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
        [InlineData("doNotExist")]
        public void Create_WhenVariationCodeIsNotValid_ShouldReturnModelWithNoVariation(string variationCode)
        {
            var fashionProduct = CreateFashionProduct();
            
            var result = CreateSubject().Create(fashionProduct, variationCode);
            
            Assert.Null(result.Variant);
        }

        [Fact]
        public void Create_WhenSelectedVariationExist_ShouldSetVariationToSelectedVariation()
        {
            var fashionProduct = CreateFashionProduct();
            var fashionVariant = CreateFashionVariant();

            SetVariantsToReturn(fashionVariant);

            var result = CreateSubject().Create(fashionProduct, fashionVariant.Code);

            Assert.Equal<FashionVariant>(fashionVariant, result.Variant);
        }

        [Fact]
        public void Create_WhenSelectedVariationExist_ShouldSetProductToProduct()
        {
            var fashionProduct = CreateFashionProduct();
            var fashionVariant = CreateFashionVariant();

            SetVariantsToReturn(fashionVariant);
            
            var result = CreateSubject().Create(fashionProduct, fashionVariant.Code);

            Assert.Equal<FashionProduct>(fashionProduct, result.Product);
        }

        [Fact]
        public void Create_WhenSelectedVariationExist_ShouldSetOriginalPriceToDefaultPrice()
        {
            var fashionProduct = CreateFashionProduct();
            var fashionVariant = CreateFashionVariant();

            SetVariantsToReturn(fashionVariant);

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

            SetVariantsToReturn(fashionVariant);

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

            SetVariantsToReturn(fashionVariant);

            var result = CreateSubject().Create(fashionProduct, fashionVariant.Code);

            Assert.Equal(color, result.Color);
        }

        [Fact]
        public void Create_WhenSelectedVariationExist_ShouldSetSizeToSelectedVariationSize()
        {
            var size = "small";
            var fashionProduct = CreateFashionProduct();
            var fashionVariant = CreateFashionVariant();
            fashionVariant.Size = size;

            SetVariantsToReturn(fashionVariant);

            var result = CreateSubject().Create(fashionProduct, fashionVariant.Code);

            Assert.Equal(size, result.Size);
        }

        [Fact]
        public void Create_WhenSelectedVariationDontHaveAssets_ShouldSetImagesToOneItemWithEmptyLink()
        {
            var fashionProduct = CreateFashionProduct();
            var fashionVariant = CreateFashionVariant();

            SetVariantsToReturn(fashionVariant);

            var result = CreateSubject().Create(fashionProduct, fashionVariant.Code);

            Assert.Equal(string.Empty, result.Images.Single());
        }

        [Fact]
        public void Create_WhenSelectedVariationHasImageAssets_ShouldSetImagesToLinkFromImage()
        {
            var imageLink = "http://www.episerver.com";

            var fashionProduct = CreateFashionProduct();
            var fashionVariant = CreateFashionVariant();
            SetVariantsToReturn(fashionVariant);

            IContentImage contentImage;

            var imageMedia = new CommerceMedia {AssetLink = new ContentReference(237)};
            _contentLoaderMock.Setup(x => x.TryGet(imageMedia.AssetLink, out contentImage)).Returns(true);
            _urlResolverMock.Setup(x => x.GetUrl(imageMedia.AssetLink)).Returns(imageLink);

            fashionVariant.CommerceMediaCollection = new ItemCollection<CommerceMedia>() {imageMedia};
           
            var result = CreateSubject().Create(fashionProduct, fashionVariant.Code);

            Assert.Equal(imageLink, result.Images.Single());
        }

        [Fact]
        public void Create_WhenAvailableColorsAreEmptyForVariation_ShouldSetColorsToEmpty()
        {
            var fashionProduct = CreateFashionProduct();
            var fashionVariant = CreateFashionVariant();

            SetVariantsToReturn(fashionVariant);

            var result = CreateSubject().Create(fashionProduct, fashionVariant.Code);

            Assert.Equal<int>(0, result.Colors.Count);
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

            SetVariantsToReturn(fashionVariant1, fashionVariant2, fashionVariant3);

            var result = CreateSubject().Create(fashionProduct, fashionVariant1.Code);

            result.Colors.Select(x => x.Text).Should().BeEquivalentTo(colors);
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

            SetVariantsToReturn(fashionVariant1, fashionVariant2, fashionVariant3);

            var result = CreateSubject().Create(fashionProduct, fashionVariant1.Code);
            
            result.Colors.Select(x => x.Text).Should().BeEquivalentTo(colors);
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

            SetVariantsToReturn(fashionVariant1, fashionVariant2, fashionVariant3);

            var result = CreateSubject().Create(fashionProduct, fashionVariant1.Code);

            result.Colors.Select(x => x.Selected).Should().BeEquivalentTo(new[] { false, false });
        }

        [Fact]
        public void Create_WhenAvailableSizesContainsItems_ShouldSetTextToItemValue()
        {
            var sizes = new ItemCollection<string>() {"medium"};

            var fashionProduct = CreateFashionProduct();
            var fashionVariant = CreateFashionVariant("medium", "red");
            SetVariantsToReturn(fashionVariant);


            var result = CreateSubject().Create(fashionProduct, fashionVariant.Code);
            
            result.Sizes.Select(x => x.Text).Should().BeEquivalentTo(sizes);
        }

        [Fact]
        public void Create_WhenAvailableSizesContainsItems_ShouldSetValueToItemValue()
        {
            var sizes = new ItemCollection<string>() {"medium"};

            var fashionProduct = CreateFashionProduct();
            var fashionVariant = CreateFashionVariant("medium", "red");
            SetVariantsToReturn(fashionVariant);

            var result = CreateSubject().Create(fashionProduct, fashionVariant.Code);

            result.Sizes.Select(x => x.Value).Should().BeEquivalentTo(sizes);
        }

        [Fact]
        public void Create_WhenAvailableSizesContainsItems_ShouldSetSelectedToFalse()
        {
            var fashionProduct = CreateFashionProduct();
            var fashionVariant = CreateFashionVariant("medium", "red");
            SetVariantsToReturn(fashionVariant);

            var result = CreateSubject().Create(fashionProduct, fashionVariant.Code);

            result.Sizes.Select(x => x.Selected).Should().BeEquivalentTo(new[] { false });
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

            SetVariantsToReturn(fashionVariantSmallBlue, fashionVariantSmallWhite);

            var result = CreateSubject().Create(fashionProduct, fashionVariantSmallBlue.Code);

            result.Colors.Select(x => x.Value).Should().BeEquivalentTo(new[] { variationColorBlue, variationColorWhite });
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

            SetVariantsToReturn(fashionVariantMediumRed, fashionVariantXlargeRed);

            var result = CreateSubject().Create(fashionProduct, fashionVariantMediumRed.Code);

            result.Sizes.Select(x => x.Value).Should().BeEquivalentTo(new[] { variationSizeMedium, variationSizeXlarge });
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

            SetVariantsToReturn(fashionVariantSmallGreen, fashionVariantSmallRed, fashionVariantMediumGreen, fashionVariantMediumRed);

            var result = CreateSubject().SelectVariant(fashionProduct, "red", "small");

            Assert.Equal("redsmall", result.Code);
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

            SetVariantsToReturn(fashionVariantSmallGreen, fashionVariantMediumRed);

            var result = CreateSubject().SelectVariant(fashionProduct, "red", "small");

            Assert.Equal("redmedium", result.Code);
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

            SetVariantsToReturn(fashionVariantSmallGreen, fashionVariantMediumRed);

            var result = CreateSubject().SelectVariant(fashionProduct, "yellow", "small");

            Assert.Null(result);
        }

        private readonly Mock<IContentLoader> _contentLoaderMock;
        private readonly Mock<IPricingService> _pricingServiceMock;
        private readonly Mock<UrlResolver> _urlResolverMock;
        private readonly Mock<CatalogContentService> _catalogContentServiceMock;
        public CatalogEntryViewModelFactoryTests()
        {
            _urlResolverMock = new Mock<UrlResolver>();
            _contentLoaderMock = new Mock<IContentLoader>();
            _pricingServiceMock = new Mock<IPricingService>();
            _catalogContentServiceMock = new Mock<CatalogContentService>(null,null,null,null,null,null,null);
        }

        private void SetVariantsToReturn(params FashionVariant[] variants)
        {
            _catalogContentServiceMock.Setup(x => x.GetVariants<FashionVariant>(It.IsAny<FashionProduct>()))
                .Returns(() => variants);
        }

        private CatalogEntryViewModelFactory CreateSubject()
        {
            return new CatalogEntryViewModelFactory(
               _contentLoaderMock.Object,
               _pricingServiceMock.Object,
               _urlResolverMock.Object,
                _catalogContentServiceMock.Object);
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

        private void SetDefaultPriceService(IPriceValue returnedPrice)
        {
            _pricingServiceMock.Setup(x => x.GetDefaultPrice(It.IsAny<string>())).Returns(returnedPrice);
        }

        private void SetDiscountPriceService(IPriceValue returnedPrice)
        {
            _pricingServiceMock.Setup(x => x.GetDiscountPrice(It.IsAny<string>())).Returns(returnedPrice);
        }

        private Mock<IPriceValue> CreatePriceValueMock(decimal amount)
        {
            var mockPriceValue = new Mock<IPriceValue>();
            mockPriceValue.Setup(x => x.ValidFrom).Returns(DateTime.MinValue);
            mockPriceValue.Setup(x => x.ValidUntil).Returns(DateTime.MaxValue);
            mockPriceValue.Setup(x => x.UnitPrice).Returns(new Money(amount, Currency.USD));

            return mockPriceValue;
        }
    }
}
