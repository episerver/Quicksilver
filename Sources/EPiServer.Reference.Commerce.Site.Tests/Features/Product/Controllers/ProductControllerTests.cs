using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.Core;
using EPiServer.Filters;
using EPiServer.Reference.Commerce.Site.Features.Market;
using EPiServer.Reference.Commerce.Site.Features.Product.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Pricing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Product.Controllers
{
    [TestClass]
    public class ProductControllerTests
    {
        [TestMethod]
        public void Index_WhenVariationIdIsNull_ShouldReturnHttpNotFoundResult()
        {
            Mock<FashionProduct> mockFashionProduct = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {
                mockFashionProduct = CreateFashionProductMock();
                SetRelation(mockFashionProduct.Object, Enumerable.Empty<ProductVariation>());

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(mockFashionProduct.Object, null);
            }

            // Assert
            {
                Assert.IsInstanceOfType(actionResult, typeof(HttpNotFoundResult));
            }
        }

        [TestMethod]
        public void Index_WhenVariationIdIsEmpty_ShouldReturnHttpNotFoundResult()
        {
            Mock<FashionProduct> mockFashionProduct = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {
                mockFashionProduct = CreateFashionProductMock();
                SetRelation(mockFashionProduct.Object, Enumerable.Empty<ProductVariation>());

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(mockFashionProduct.Object, string.Empty);
            }

            // Assert
            {
                Assert.IsInstanceOfType(actionResult, typeof(HttpNotFoundResult));
            }
        }

        [TestMethod]
        public void Index_WhenNoVariationExists_ShouldReturnHttpNotFoundResult()
        {
            Mock<FashionProduct> mockFashionProduct = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {
                mockFashionProduct = CreateFashionProductMock();
                SetRelation(mockFashionProduct.Object, Enumerable.Empty<ProductVariation>());

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(mockFashionProduct.Object, "something");
            }

            // Assert
            {
                Assert.IsInstanceOfType(actionResult, typeof (HttpNotFoundResult));
            }
        }

        [TestMethod]
        public void Index_WhenSelectedVariationDontExist_ShouldReturnHttpNotFoundResult()
        {
            Mock<FashionProduct> mockFashionProduct = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {
                mockFashionProduct = CreateFashionProductMock();
                var mockFashionVariant = CreateFashionVariantMock();
                SetRelation(mockFashionProduct.Object, mockFashionVariant.Object);

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(mockFashionProduct.Object, "doNotExist");
            }

            // Assert
            {
                Assert.IsInstanceOfType(actionResult, typeof (HttpNotFoundResult));
            }
        }

        [TestMethod]
        public void Index_WhenSelectedVariationExist_ShouldSetVariationToSelectedVariation()
        {
            Mock<FashionProduct> mockFashionProduct = null;
            Mock<FashionVariant> mockFashionVariant = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // setup
            {
                mockFashionProduct = CreateFashionProductMock();
                mockFashionVariant = CreateFashionVariantMock();
                SetRelation(mockFashionProduct.Object, mockFashionVariant.Object);

                productController = CreateController();
            }

            // execute
            {
                actionResult = productController.Index(mockFashionProduct.Object, mockFashionVariant.Object.Code);
            }

            // Assert
            {
                var model = (FashionProductViewModel)((ViewResultBase)actionResult).Model;
                Assert.AreEqual<FashionVariant>(mockFashionVariant.Object, model.Variation);
            }
        }

        [TestMethod]
        public void Index_WhenSelectedVariationExist_ShouldSetProductToRoutedProduct()
        {
            Mock<FashionProduct> mockFashionProduct = null;
            Mock<FashionVariant> mockFashionVariant = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {
                mockFashionProduct = CreateFashionProductMock();
                mockFashionVariant = CreateFashionVariantMock();
                SetRelation(mockFashionProduct.Object, mockFashionVariant.Object);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(mockFashionProduct.Object, mockFashionVariant.Object.Code);
            }

            // Assert
            {
                var model = (FashionProductViewModel)((ViewResultBase)actionResult).Model;
                Assert.AreEqual<FashionProduct>(mockFashionProduct.Object, model.Product);
            }
        }

        [TestMethod]
        public void Index_WhenSelectedVariationExist_ShouldSetOriginalPriceToDefaultPrice()
        {
            Mock<FashionProduct> mockFashionProduct = null;
            Mock<FashionVariant> mockFashionVariant = null;
            ProductController productController = null;
            Mock<IPriceValue> mockDefaultPrice = null;
            ActionResult actionResult = null;

            // Setup
            {
                mockFashionProduct = CreateFashionProductMock();
                mockFashionVariant = CreateFashionVariantMock();
                SetRelation(mockFashionProduct.Object, mockFashionVariant.Object);

                mockDefaultPrice = CreatePriceValueMock(25);
                SetDefaultPriceService(mockDefaultPrice.Object);

                var mockDiscountPrice = CreatePriceValueMock(20);
                SetDiscountPriceService(mockDiscountPrice.Object);

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(mockFashionProduct.Object, mockFashionVariant.Object.Code);
            }

            // Assert
            {
                var model = (FashionProductViewModel)((ViewResultBase)actionResult).Model;
                Assert.AreEqual<Money>(mockDefaultPrice.Object.UnitPrice, model.OriginalPrice);
            }
        }

        [TestMethod]
        public void Index_WhenSelectedVariationExist_ShouldSetPriceToDiscountPrice()
        {
            Mock<FashionProduct> mockFashionProduct = null;
            Mock<FashionVariant> mockFashionVariant = null;
            ProductController productController = null;
            Mock<IPriceValue> mockDiscountPrice = null;
            ActionResult actionResult = null;

            // Setup
            {
                mockFashionProduct = CreateFashionProductMock();
                mockFashionVariant = CreateFashionVariantMock();
                SetRelation(mockFashionProduct.Object, mockFashionVariant.Object);

                var mockDefaultPrice = CreatePriceValueMock(25);
                SetDefaultPriceService(mockDefaultPrice.Object);

                mockDiscountPrice = CreatePriceValueMock(20);
                SetDiscountPriceService(mockDiscountPrice.Object);

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(mockFashionProduct.Object, mockFashionVariant.Object.Code);
            }

            // Assert
            {
                var model = (FashionProductViewModel)((ViewResultBase)actionResult).Model;
                Assert.AreEqual<Money>(mockDiscountPrice.Object.UnitPrice, model.Price);
            }
        }

        [TestMethod]
        public void Index_WhenSelectedVariationExist_ShouldSetColorToSelectedVariationColor()
        {
            Mock<FashionProduct> mockFashionProduct = null;
            Mock<FashionVariant> mockFashionVariant = null;
            ProductController productController = null;
            string color = null;
            ActionResult actionResult = null;

            // Setup
            {
                color = "green";

                mockFashionProduct = CreateFashionProductMock();
                mockFashionVariant = CreateFashionVariantMock();
                SetColor(mockFashionVariant, color);

                SetRelation(mockFashionProduct.Object, mockFashionVariant.Object);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(mockFashionProduct.Object, mockFashionVariant.Object.Code);
            }

            // Assert
            {
                var model = (FashionProductViewModel)((ViewResultBase)actionResult).Model;
                Assert.AreEqual<string>(color, model.Color);
            }
        }

        [TestMethod]
        public void Index_WhenSelectedVariationExist_ShouldSetSizeToSelectedVariationSize()
        {
            Mock<FashionProduct> mockFashionProduct = null;
            Mock<FashionVariant> mockFashionVariant = null;
            ProductController productController = null;
            string size = null;
            ActionResult actionResult = null;

            // Setup
            {
                size = "small";

                mockFashionProduct = CreateFashionProductMock();
                mockFashionVariant = CreateFashionVariantMock();
                SetSize(mockFashionVariant, size);

                SetRelation(mockFashionProduct.Object, mockFashionVariant.Object);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(mockFashionProduct.Object, mockFashionVariant.Object.Code);
            }

            // Assert
            {
                var model = (FashionProductViewModel)((ViewResultBase)actionResult).Model;
                Assert.AreEqual<string>(size, model.Size);
            }
        }

        [TestMethod]
        public void Index_WhenSelectedVariationDontHaveAssets_ShouldSetImagesToOneItemWithEmptyLink()
        {
            Mock<FashionProduct> mockFashionProduct = null;
            Mock<FashionVariant> mockFashionVariant = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {
                mockFashionProduct = CreateFashionProductMock();
                mockFashionVariant = CreateFashionVariantMock();

                SetRelation(mockFashionProduct.Object, mockFashionVariant.Object);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(mockFashionProduct.Object, mockFashionVariant.Object.Code);
            }

            // Assert
            {
                var model = (FashionProductViewModel)((ViewResultBase)actionResult).Model;
                Assert.AreEqual<string>(string.Empty, model.Images.Single());
            }
        }

        [TestMethod]
        public void Index_WhenSelectedVariationHasImageAssets_ShouldSetImagesToLinkFromImage()
        {
            Mock<FashionProduct> mockFashionProduct = null;
            Mock<FashionVariant> mockFashionVariant = null;
            ProductController productController = null;
            string imageLink = null;
            ActionResult actionResult = null;

            // Setup
            {
                imageLink = "http://www.episerver.com";

                mockFashionProduct = CreateFashionProductMock();
                mockFashionVariant = CreateFashionVariantMock();

                var imageMedia = CreateImageMedia(new ContentReference(237), imageLink);
                SetMediaCollection(mockFashionVariant, imageMedia);

                SetRelation(mockFashionProduct.Object, mockFashionVariant.Object);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(mockFashionProduct.Object, mockFashionVariant.Object.Code);
            }

            // Assert
            {
                var model = (FashionProductViewModel)((ViewResultBase)actionResult).Model;
                Assert.AreEqual<string>(imageLink, model.Images.Single());
            }
        }

        [TestMethod]
        public void Index_WhenAvailableColorsAreEmptyForVariation_ShouldSetColorsToEmpty()
        {
            Mock<FashionProduct> mockFashionProduct = null;
            Mock<FashionVariant> mockFashionVariant = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {

                mockFashionProduct = CreateFashionProductMock();

                mockFashionVariant = CreateFashionVariantMock();
                SetRelation(mockFashionProduct.Object, mockFashionVariant.Object);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(mockFashionProduct.Object, mockFashionVariant.Object.Code);
            }

            // Assert
            {
                var model = (FashionProductViewModel)((ViewResultBase)actionResult).Model;
                Assert.AreEqual<int>(0, model.Colors.Count());
            }
        }

        [TestMethod]
        public void Index_WhenAvailableColorsContainsItems_ShouldSetTextToItemValue()
        {
            Mock<FashionProduct> mockFashionProduct = null;
            Mock<FashionVariant> mockFashionVariant = null;
            ProductController productController = null;
            ItemCollection<string> colors = null;
            ActionResult actionResult = null;

            // Setup
            {
                colors = new ItemCollection<string>() {"green", "white"};

                mockFashionProduct = CreateFashionProductMock();
                SetAvailableColors(mockFashionProduct, colors);

                mockFashionVariant = CreateFashionVariantMock();
                SetRelation(mockFashionProduct.Object, mockFashionVariant.Object);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(mockFashionProduct.Object, mockFashionVariant.Object.Code);
            }

            // Assert
            {
                var model = (FashionProductViewModel)((ViewResultBase)actionResult).Model;
                var expectedColors = String.Join(";", colors);
                var modelColorTexts = String.Join(";", model.Colors.Select(x => x.Text));

                Assert.AreEqual<string>(expectedColors, modelColorTexts);
            }
        }

        [TestMethod]
        public void Index_WhenAvailableColorsContainsItems_ShouldSetValueToItemValue()
        {
            Mock<FashionProduct> mockFashionProduct = null;
            Mock<FashionVariant> mockFashionVariant = null;
            ProductController productController = null;
            ItemCollection<string> colors = null;
            ActionResult actionResult = null;

            // Setup
            {
                colors = new ItemCollection<string>() { "green", "white" };

                mockFashionProduct = CreateFashionProductMock();
                SetAvailableColors(mockFashionProduct, colors);

                mockFashionVariant = CreateFashionVariantMock();
                SetRelation(mockFashionProduct.Object, mockFashionVariant.Object);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(mockFashionProduct.Object, mockFashionVariant.Object.Code);
            }

            // Assert
            {
                var model = (FashionProductViewModel)((ViewResultBase)actionResult).Model;
                var expectedColors = String.Join(";", colors);
                var modelColorValues = String.Join(";", model.Colors.Select(x => x.Value));

                Assert.AreEqual<string>(expectedColors, modelColorValues);
            }
        }

        [TestMethod]
        public void Index_WhenAvailableColorsContainsItems_ShouldSetSelectedToFalse()
        {
            Mock<FashionProduct> mockFashionProduct = null;
            Mock<FashionVariant> mockFashionVariant = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {

                mockFashionProduct = CreateFashionProductMock();
                SetAvailableColors(mockFashionProduct, new ItemCollection<string>() { "green", "white" });

                mockFashionVariant = CreateFashionVariantMock();
                SetRelation(mockFashionProduct.Object, mockFashionVariant.Object);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(mockFashionProduct.Object, mockFashionVariant.Object.Code);
            }

            // Assert
            {
                var model = (FashionProductViewModel)((ViewResultBase)actionResult).Model;
                var expectedColors = String.Join(";", new[]{false, false});
                var modelColorsSelected = String.Join(";", model.Colors.Select(x => x.Selected));

                Assert.AreEqual<string>(expectedColors, modelColorsSelected);
            }
        }

        [TestMethod]
        public void Index_WhenAvailableSizesContainsItems_ShouldSetTextToItemValue()
        {
            Mock<FashionProduct> mockFashionProduct = null;
            Mock<FashionVariant> mockFashionVariant = null;
            ProductController productController = null;
            ItemCollection<string> sizes = null;
            ActionResult actionResult = null;

            // Setup
            {
                sizes = new ItemCollection<string>() {"medium" };

                mockFashionProduct = CreateFashionProductMock();
                mockFashionVariant = CreateFashionVariantMock();
                SetColor(mockFashionVariant, "Red");
                SetSize(mockFashionVariant, "medium");
                SetRelation(mockFashionProduct.Object, mockFashionVariant.Object);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(mockFashionProduct.Object, mockFashionVariant.Object.Code);
            }

            // Assert
            {
                var model = (FashionProductViewModel)((ViewResultBase)actionResult).Model;
                var expectedSizes = String.Join(";", sizes);
                var modelSizeTexts = String.Join(";", model.Sizes.Select(x => x.Text));

                Assert.AreEqual<string>(expectedSizes, modelSizeTexts);
            }
        }

        [TestMethod]
        public void Index_WhenAvailableSizesContainsItems_ShouldSetValueToItemValue()
        {
            Mock<FashionProduct> mockFashionProduct = null;
            Mock<FashionVariant> mockFashionVariant = null;
            ProductController productController = null;
            ItemCollection<string> sizes = null;
            ActionResult actionResult = null;

            // Setup
            {
                sizes = new ItemCollection<string>() { "medium" };

                mockFashionProduct = CreateFashionProductMock();
                mockFashionVariant = CreateFashionVariantMock();
                SetColor(mockFashionVariant, "Red");
                SetSize(mockFashionVariant, "medium");
                SetRelation(mockFashionProduct.Object, mockFashionVariant.Object);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(mockFashionProduct.Object, mockFashionVariant.Object.Code);
            }

            // Assert
            {
                var model = (FashionProductViewModel)((ViewResultBase)actionResult).Model;
                var expectedSizes = String.Join(";", sizes);
                var modelSizeValues = String.Join(";", model.Sizes.Select(x => x.Value));

                Assert.AreEqual<string>(expectedSizes, modelSizeValues);
            }
        }

        [TestMethod]
        public void Index_WhenAvailableSizesContainsItems_ShouldSetSelectedToFalse()
        {
            Mock<FashionProduct> mockFashionProduct = null;
            Mock<FashionVariant> mockFashionVariant = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {

                mockFashionProduct = CreateFashionProductMock();
                mockFashionVariant = CreateFashionVariantMock();
                SetColor(mockFashionVariant, "Red");
                SetSize(mockFashionVariant, "medium");
                SetRelation(mockFashionProduct.Object, mockFashionVariant.Object);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(mockFashionProduct.Object, mockFashionVariant.Object.Code);
            }

            // Assert
            {
                var model = (FashionProductViewModel)((ViewResultBase)actionResult).Model;
                var expectedSizes = String.Join(";", new[] { false });
                var modelSizesSelected = String.Join(";", model.Sizes.Select(x => x.Selected));

                Assert.AreEqual<string>(expectedSizes, modelSizesSelected);
            }
        }

        [TestMethod]
        public void Index_WhenAvailableSizesContainsDelayPublishItems_ShouldReturnHttpNotFoundResult()
        {
            Mock<FashionProduct> mockFashionProduct = null;
            Mock<FashionVariant> mockFashionVariant = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {
                var sizes = new ItemCollection<string>() { "small", "medium" };

                mockFashionProduct = CreateFashionProductMock();
                SetAvailableSizes(mockFashionProduct, sizes);
                 
                // setup variant with publish date in future
                mockFashionVariant = CreateFashionVariantMock();
                mockFashionVariant.Setup(x => x.StartPublish).Returns(DateTime.UtcNow.AddDays(7)); // pulish date is future
                mockFashionVariant.Setup(x => x.StopPublish).Returns(DateTime.UtcNow.AddDays(17));
                mockFashionVariant.Setup(x => x.Status).Returns(VersionStatus.DelayedPublish);
                
                SetRelation(mockFashionProduct.Object, mockFashionVariant.Object);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(mockFashionProduct.Object, mockFashionVariant.Object.Code);
            }

            // Assert
            {
                Assert.IsInstanceOfType(actionResult, typeof(HttpNotFoundResult));
            }
        }

        [TestMethod]
        public void Index_WhenAvailableSizesContainsExpiredItems_ShouldReturnHttpNotFoundResult()
        {
            Mock<FashionProduct> mockFashionProduct = null;
            Mock<FashionVariant> mockFashionVariant = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {
                var sizes = new ItemCollection<string>() { "small", "medium" };

                mockFashionProduct = CreateFashionProductMock();
                SetAvailableSizes(mockFashionProduct, sizes);

                // setup variant was expired.
                mockFashionVariant = CreateFashionVariantMock();
                mockFashionVariant.Setup(x => x.StartPublish).Returns(DateTime.UtcNow.AddDays(-17)); 
                mockFashionVariant.Setup(x => x.StopPublish).Returns(DateTime.UtcNow.AddDays(-7));

                SetRelation(mockFashionProduct.Object, mockFashionVariant.Object);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(mockFashionProduct.Object, mockFashionVariant.Object.Code);
            }

            // Assert
            {
                Assert.IsInstanceOfType(actionResult, typeof(HttpNotFoundResult));
            }
        }

        [TestMethod]
        public void Index_WhenAvailableSizesContainsUnpublishItems_ShouldReturnHttpNotFoundResult()
        {
            Mock<FashionProduct> mockFashionProduct = null;
            Mock<FashionVariant> mockFashionVariant = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {
                var sizes = new ItemCollection<string>() { "small", "medium" };

                mockFashionProduct = CreateFashionProductMock();
                SetAvailableSizes(mockFashionProduct, sizes);

                // setup variant with inactive
                mockFashionVariant = CreateFashionVariantMock();
                mockFashionVariant.Setup(x => x.IsPendingPublish).Returns(true);
                mockFashionVariant.Setup(x => x.Status).Returns(VersionStatus.CheckedIn);

                SetRelation(mockFashionProduct.Object, mockFashionVariant.Object);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            { 
                actionResult = productController.Index(mockFashionProduct.Object, mockFashionVariant.Object.Code);
            }

            // Assert
            {
                Assert.IsInstanceOfType(actionResult, typeof(HttpNotFoundResult));
            }
        }

        [TestMethod]
        public void Index_WhenAvailableSizesContainsItemUnavailabelInCurrentMarket_ShouldReturnHttpNotFoundResult()
        {
            Mock<FashionProduct> mockFashionProduct = null;
            Mock<FashionVariant> mockFashionVariant = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {
                var sizes = new ItemCollection<string>() { "small", "medium" };

                mockFashionProduct = CreateFashionProductMock();
                SetAvailableSizes(mockFashionProduct, sizes);

                // setup variant unavailable in default market
                mockFashionVariant = CreateFashionVariantMock();
                mockFashionVariant.Setup(x => x.MarketFilter).Returns(new ItemCollection<string>() { "Default" });  

                SetRelation(mockFashionProduct.Object, mockFashionVariant.Object);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(mockFashionProduct.Object, mockFashionVariant.Object.Code);
            }

            // Assert
            {
                Assert.IsInstanceOfType(actionResult, typeof(HttpNotFoundResult));
            }
        }

        private Mock<IPromotionService> _mockPromotionService;
        private Mock<IContentLoader> _mockContentLoader;
        private Mock<IPriceService> _mockPriceService;
        private Mock<ICurrentMarket> _mockCurrentMarket;
        private FilterPublished _filterPublished;
        private Mock<CurrencyService> _mockCurrencyservice;
        private Mock<IRelationRepository> _mockRelationRepository;
        private Mock<CookieService> _mockCookieService;
        private Mock<UrlResolver> _mockUrlResolver;

        private Mock<HttpContextBase> _mockHttpContextBase;
        private Mock<IMarket> _mockMarket;
        private Mock<AppContextFacade> _appContextFacade;

        private Currency _defaultCurrency;
        private CultureInfo _preferredCulture;

        [TestInitialize]
        public void Setup()
        {
            _defaultCurrency = Currency.USD;
            _preferredCulture = CultureInfo.GetCultureInfo("en");

            _mockUrlResolver = new Mock<UrlResolver>();
            _mockContentLoader = new Mock<IContentLoader>();
            _mockCookieService = new Mock<CookieService>();
            _mockPriceService = new Mock<IPriceService>();
            _mockRelationRepository = new Mock<IRelationRepository>();
            _mockPromotionService = new Mock<IPromotionService>();

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

            _mockMarket = new Mock<IMarket>();
            _mockMarket.Setup(x => x.DefaultCurrency).Returns(_defaultCurrency);
            _mockMarket.Setup(x => x.MarketId).Returns(new MarketId("Default"));
            _mockMarket.Setup(x => x.MarketName).Returns("Default");
            _mockMarket.Setup(x => x.IsEnabled).Returns(true);
            _mockMarket.Setup(x => x.DefaultLanguage).Returns(new CultureInfo("en"));

            _mockCurrentMarket = new Mock<ICurrentMarket>();
            _mockCurrentMarket.Setup(x => x.GetCurrentMarket()).Returns(_mockMarket.Object);

            _mockCurrencyservice = new Mock<CurrencyService>(_mockCurrentMarket.Object, _mockCookieService.Object);
            _mockCurrencyservice.Setup(x => x.GetCurrentCurrency()).Returns(_defaultCurrency);

            _appContextFacade = new Mock<AppContextFacade>();
            _appContextFacade.Setup(x => x.ApplicationId).Returns(Guid.NewGuid);

            var request = new Mock<HttpRequestBase>();
            request.SetupGet(x => x.Headers).Returns(
                new System.Net.WebHeaderCollection {
                {"X-Requested-With", "XMLHttpRequest"}
            });

            _mockHttpContextBase = new Mock<HttpContextBase>();
            _mockHttpContextBase.SetupGet(x => x.Request).Returns(request.Object);

            SetGetItems(Enumerable.Empty<ContentReference>(), Enumerable.Empty<IContent>());
            SetDefaultCurrency(null);
        }

        private ProductController CreateController()
        {
            var controller = new ProductController(
                _mockPromotionService.Object,
                _mockContentLoader.Object,
                _mockPriceService.Object,
                _mockCurrentMarket.Object,
                _mockCurrencyservice.Object,
                _mockRelationRepository.Object,
                _appContextFacade.Object,
                _mockUrlResolver.Object,
                _filterPublished,
                () => _preferredCulture);

            controller.ControllerContext = new ControllerContext(_mockHttpContextBase.Object, new RouteData(), controller);

            return controller;
        }

        private static Mock<FashionVariant> CreateFashionVariantMock()
        {
            var mockFashionVariant = new Mock<FashionVariant>();
            mockFashionVariant.Setup(x => x.ContentLink).Returns(new ContentReference(740));
            mockFashionVariant.Setup(x => x.Code).Returns("myVariant");
            mockFashionVariant.Setup(x => x.IsDeleted).Returns(false);
            mockFashionVariant.Setup(x => x.IsPendingPublish).Returns(false);
            mockFashionVariant.Setup(x => x.Status).Returns(VersionStatus.Published);
            mockFashionVariant.Setup(x => x.StartPublish).Returns(DateTime.UtcNow.AddDays(-7));
            mockFashionVariant.Setup(x => x.StopPublish).Returns(DateTime.UtcNow.AddDays(7));
            mockFashionVariant.Setup(x => x.MarketFilter).Returns(new ItemCollection<string>() { "USA" });  

            return mockFashionVariant;
        }

        private static Mock<FashionProduct> CreateFashionProductMock()
        {
            var mockFashionProduct = new Mock<FashionProduct>();
            mockFashionProduct.Setup(x => x.ContentLink).Returns(new ContentReference(741));
            mockFashionProduct.Setup(x => x.Code).Returns("myProduct");
            mockFashionProduct.Setup(x => x.IsDeleted).Returns(false);
            mockFashionProduct.Setup(x => x.IsPendingPublish).Returns(false);
            mockFashionProduct.Setup(x => x.Status).Returns(VersionStatus.Published);
            mockFashionProduct.Setup(x => x.StartPublish).Returns(DateTime.UtcNow.AddDays(-7));
            mockFashionProduct.Setup(x => x.StopPublish).Returns(DateTime.UtcNow.AddDays(7));
            mockFashionProduct.Setup(x => x.MarketFilter).Returns(new ItemCollection<string>() { "USA" });   

            SetAvailableColors(mockFashionProduct, new ItemCollection<string>());
            SetAvailableSizes(mockFashionProduct, new ItemCollection<string>());

            return mockFashionProduct;
        }

        private static void SetAvailableColors(Mock<FashionProduct> product, ItemCollection<string> colors)
        {
            product.Setup(x => x.AvailableColors).Returns(colors);
        }

        private static void SetAvailableSizes(Mock<FashionProduct> product, ItemCollection<string> sizes)
        {
            product.Setup(x => x.AvailableSizes).Returns(sizes);
        }

        private static void SetColor(Mock<FashionVariant> mockFashionVariant, string color)
        {
            mockFashionVariant.Setup(x => x.Color).Returns(color);
        }

        private static void SetSize(Mock<FashionVariant> mockFashionVariant, string size)
        {
            mockFashionVariant.Setup(x => x.Size).Returns(size);
        }

        private void SetRelation(IContent source, IContent target)
        {
            var mockProductVariationRelation = new ProductVariation
            {
                Source = source.ContentLink,
                Target = target.ContentLink
            };

            SetRelation(source, new[] { mockProductVariationRelation });

            SetGetItems(new[] { source.ContentLink }, new[] { source });
            SetGetItems(new[] { target.ContentLink }, new[] { target });
        }

        private void SetRelation(IContent setup, IEnumerable<ProductVariation> result)
        {
            _mockRelationRepository.Setup(x => x.GetRelationsBySource<ProductVariation>(setup.ContentLink)).Returns(result);
        }

        private void SetGetItems(IEnumerable<ContentReference> setup, IEnumerable<IContent> result)
        {
            _mockContentLoader.Setup(x => x.GetItems(setup, _preferredCulture)).Returns(result);
        }

        private void SetDefaultCurrency(string currency)
        {
            _mockCookieService.Setup(x => x.Get("Currency")).Returns(currency);
        }

        private void SetDefaultPriceService(IPriceValue returnedPrice)
        {
            _mockPriceService
                .Setup(x => x.GetDefaultPrice(It.IsAny<MarketId>(), It.IsAny<DateTime>(), It.IsAny<CatalogKey>(), _defaultCurrency))
                .Returns(returnedPrice);
        }

        private void SetDiscountPriceService(IPriceValue returnedPrice)
        {
            _mockPromotionService
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

        private CommerceMedia CreateImageMedia(ContentReference contentLink, string imageLink)
        {
            IContentImage contentImage;

            var imageMedia = new CommerceMedia { AssetLink = contentLink };
            _mockContentLoader.Setup(x => x.TryGet(imageMedia.AssetLink, out contentImage)).Returns(true);
            _mockUrlResolver.Setup(x => x.GetUrl(imageMedia.AssetLink)).Returns(imageLink);

            return imageMedia;
        }

        private static void SetMediaCollection(Mock<FashionVariant> mockFashionVariant, CommerceMedia media)
        {
            mockFashionVariant.Setup(x => x.CommerceMediaCollection).Returns(new ItemCollection<CommerceMedia>() { media });
        }
    }
}
