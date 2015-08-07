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
using FluentAssertions;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Product.Controllers
{
    [TestClass]
    public class ProductControllerTests
    {
        [TestMethod]
        public void Index_WhenVariationIdIsNull_ShouldReturnHttpNotFoundResult()
        {
            FashionProduct fashionProduct = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {
                fashionProduct = CreateFashionProduct();
                SetRelation(fashionProduct, Enumerable.Empty<ProductVariation>());

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(fashionProduct, null);
            }

            // Assert
            {
                Assert.IsInstanceOfType(actionResult, typeof(HttpNotFoundResult));
            }
        }

        [TestMethod]
        public void Index_WhenVariationIdIsEmpty_ShouldReturnHttpNotFoundResult()
        {
            FashionProduct fashionProduct = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {
                fashionProduct = CreateFashionProduct();
                SetRelation(fashionProduct, Enumerable.Empty<ProductVariation>());

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(fashionProduct, string.Empty);
            }

            // Assert
            {
                Assert.IsInstanceOfType(actionResult, typeof(HttpNotFoundResult));
            }
        }

        [TestMethod]
        public void Index_WhenNoVariationExists_ShouldReturnHttpNotFoundResult()
        {
            FashionProduct fashionProduct = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {
                fashionProduct = CreateFashionProduct();
                SetRelation(fashionProduct, Enumerable.Empty<ProductVariation>());

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(fashionProduct, "something");
            }

            // Assert
            {
                Assert.IsInstanceOfType(actionResult, typeof (HttpNotFoundResult));
            }
        }

        [TestMethod]
        public void Index_WhenSelectedVariationDontExist_ShouldReturnHttpNotFoundResult()
        {
            FashionProduct fashionProduct = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {
                fashionProduct = CreateFashionProduct();
                var fashionVariant = CreateFashionVariant();
                SetRelation(fashionProduct, fashionVariant);

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(fashionProduct, "doNotExist");
            }

            // Assert
            {
                Assert.IsInstanceOfType(actionResult, typeof (HttpNotFoundResult));
            }
        }

        [TestMethod]
        public void Index_WhenSelectedVariationExist_ShouldSetVariationToSelectedVariation()
        {
            FashionProduct fashionProduct = null;
            FashionVariant fashionVariant = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // setup
            {
                fashionProduct = CreateFashionProduct();
                fashionVariant = CreateFashionVariant();
                SetRelation(fashionProduct, fashionVariant);

                productController = CreateController();
            }

            // execute
            {
                actionResult = productController.Index(fashionProduct, fashionVariant.Code);
            }

            // Assert
            {
                var model = (FashionProductViewModel)((ViewResultBase)actionResult).Model;
                Assert.AreEqual<FashionVariant>(fashionVariant, model.Variation);
            }
        }

        [TestMethod]
        public void Index_WhenSelectedVariationExist_ShouldSetProductToRoutedProduct()
        {
            FashionProduct fashionProduct = null;
            FashionVariant fashionVariant = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {
                fashionProduct = CreateFashionProduct();
                fashionVariant = CreateFashionVariant();
                SetRelation(fashionProduct, fashionVariant);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(fashionProduct, fashionVariant.Code);
            }

            // Assert
            {
                var model = (FashionProductViewModel)((ViewResultBase)actionResult).Model;
                Assert.AreEqual<FashionProduct>(fashionProduct, model.Product);
            }
        }

        [TestMethod]
        public void Index_WhenSelectedVariationExist_ShouldSetOriginalPriceToDefaultPrice()
        {
            FashionProduct fashionProduct = null;
            FashionVariant fashionVariant = null;
            ProductController productController = null;
            Mock<IPriceValue> mockDefaultPrice = null;
            ActionResult actionResult = null;

            // Setup
            {
                fashionProduct = CreateFashionProduct();
                fashionVariant = CreateFashionVariant();
                SetRelation(fashionProduct, fashionVariant);

                mockDefaultPrice = CreatePriceValueMock(25);
                SetDefaultPriceService(mockDefaultPrice.Object);

                var mockDiscountPrice = CreatePriceValueMock(20);
                SetDiscountPriceService(mockDiscountPrice.Object);

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(fashionProduct, fashionVariant.Code);
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
            FashionProduct fashionProduct = null;
            FashionVariant fashionVariant = null;
            ProductController productController = null;
            Mock<IPriceValue> mockDiscountPrice = null;
            ActionResult actionResult = null;

            // Setup
            {
                fashionProduct = CreateFashionProduct();
                fashionVariant = CreateFashionVariant();
                SetRelation(fashionProduct, fashionVariant);

                var mockDefaultPrice = CreatePriceValueMock(25);
                SetDefaultPriceService(mockDefaultPrice.Object);

                mockDiscountPrice = CreatePriceValueMock(20);
                SetDiscountPriceService(mockDiscountPrice.Object);

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(fashionProduct, fashionVariant.Code);
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
            FashionProduct fashionProduct = null;
            FashionVariant fashionVariant = null;
            ProductController productController = null;
            string color = null;
            ActionResult actionResult = null;

            // Setup
            {
                color = "green";

                fashionProduct = CreateFashionProduct();
                fashionVariant = CreateFashionVariant();
                SetColor(fashionVariant, color);

                SetRelation(fashionProduct, fashionVariant);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(fashionProduct, fashionVariant.Code);
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
            FashionProduct fashionProduct = null;
            FashionVariant fashionVariant = null;
            ProductController productController = null;
            string size = null;
            ActionResult actionResult = null;

            // Setup
            {
                size = "small";

                fashionProduct = CreateFashionProduct();
                fashionVariant = CreateFashionVariant();
                SetSize(fashionVariant, size);

                SetRelation(fashionProduct, fashionVariant);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(fashionProduct, fashionVariant.Code);
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
            FashionProduct fashionProduct = null;
            FashionVariant fashionVariant = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {
                fashionProduct = CreateFashionProduct();
                fashionVariant = CreateFashionVariant();

                SetRelation(fashionProduct, fashionVariant);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(fashionProduct, fashionVariant.Code);
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
            FashionProduct fashionProduct = null;
            FashionVariant fashionVariant = null;
            ProductController productController = null;
            string imageLink = null;
            ActionResult actionResult = null;

            // Setup
            {
                imageLink = "http://www.episerver.com";

                fashionProduct = CreateFashionProduct();
                fashionVariant = CreateFashionVariant();

                var imageMedia = CreateImageMedia(new ContentReference(237), imageLink);
                SetMediaCollection(fashionVariant, imageMedia);

                SetRelation(fashionProduct, fashionVariant);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(fashionProduct, fashionVariant.Code);
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
            FashionProduct fashionProduct = null;
            FashionVariant fashionVariant = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {

                fashionProduct = CreateFashionProduct();

                fashionVariant = CreateFashionVariant();
                SetRelation(fashionProduct, fashionVariant);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(fashionProduct, fashionVariant.Code);
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
            FashionProduct fashionProduct = null;
            FashionVariant fashionVariant = null;
            ProductController productController = null;
            ItemCollection<string> colors = null;
            ActionResult actionResult = null;

            // Setup
            {
                colors = new ItemCollection<string>() {"green"};

                fashionProduct = CreateFashionProduct();
                SetAvailableColors(fashionProduct, colors);
                SetAvailableSizes(fashionProduct, new ItemCollection<string> { "small" });

                fashionVariant = CreateFashionVariant("small", "green");

                SetRelation(fashionProduct, fashionVariant);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(fashionProduct, fashionVariant.Code);
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
            FashionProduct fashionProduct = null;
            FashionVariant fashionVariant = null;
            ProductController productController = null;
            ItemCollection<string> colors = null;
            ActionResult actionResult = null;

            // Setup
            {
                colors = new ItemCollection<string>() { "green" };

                fashionProduct = CreateFashionProduct();
                SetAvailableColors(fashionProduct, colors);
                SetAvailableSizes(fashionProduct, new ItemCollection<string> {"small"});

                fashionVariant = CreateFashionVariant("small", "green");

                SetRelation(fashionProduct, fashionVariant);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(fashionProduct, fashionVariant.Code);
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
            FashionProduct fashionProduct = null;
            FashionVariant fashionVariant = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {

                fashionProduct = CreateFashionProduct();
                SetAvailableColors(fashionProduct, new ItemCollection<string>() { "green" });
                SetAvailableSizes(fashionProduct, new ItemCollection<string> { "small" });

                fashionVariant = CreateFashionVariant("small", "green");

                SetRelation(fashionProduct, fashionVariant);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(fashionProduct, fashionVariant.Code);
            }

            // Assert
            {
                var model = (FashionProductViewModel)((ViewResultBase)actionResult).Model;
                var expectedColors = String.Join(";", new[]{false});
                var modelColorsSelected = String.Join(";", model.Colors.Select(x => x.Selected));

                Assert.AreEqual<string>(expectedColors, modelColorsSelected);
            }
        }

        [TestMethod]
        public void Index_WhenAvailableSizesContainsItems_ShouldSetTextToItemValue()
        {
            FashionProduct fashionProduct = null;
            FashionVariant fashionVariant = null;
            ProductController productController = null;
            ItemCollection<string> sizes = null;
            ActionResult actionResult = null;

            // Setup
            {
                sizes = new ItemCollection<string>() {"medium" };

                fashionProduct = CreateFashionProduct();
                fashionVariant = CreateFashionVariant("medium", "red");
                SetRelation(fashionProduct, fashionVariant);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(fashionProduct, fashionVariant.Code);
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
            FashionProduct fashionProduct = null;
            FashionVariant fashionVariant = null;
            ProductController productController = null;
            ItemCollection<string> sizes = null;
            ActionResult actionResult = null;

            // Setup
            {
                sizes = new ItemCollection<string>() { "medium" };

                fashionProduct = CreateFashionProduct();
                fashionVariant = CreateFashionVariant("medium", "red");
                SetRelation(fashionProduct, fashionVariant);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(fashionProduct, fashionVariant.Code);
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
            FashionProduct fashionProduct = null;
            FashionVariant fashionVariant = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {

                fashionProduct = CreateFashionProduct();
                fashionVariant = CreateFashionVariant("medium", "red");
                SetRelation(fashionProduct, fashionVariant);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(fashionProduct, fashionVariant.Code);
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
            FashionProduct fashionProduct = null;
            FashionVariant fashionVariant = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {
                var sizes = new ItemCollection<string>() { "small", "medium" };

                fashionProduct = CreateFashionProduct();
                SetAvailableSizes(fashionProduct, sizes);
                 
                // setup variant with publish date in future
                fashionVariant = CreateFashionVariant();
                fashionVariant.StartPublish = DateTime.UtcNow.AddDays(7); // pulish date is future
                fashionVariant.StopPublish = DateTime.UtcNow.AddDays(17);
                fashionVariant.Status = VersionStatus.DelayedPublish;
                
                SetRelation(fashionProduct, fashionVariant);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(fashionProduct, fashionVariant.Code);
            }

            // Assert
            {
                Assert.IsInstanceOfType(actionResult, typeof(HttpNotFoundResult));
            }
        }

        [TestMethod]
        public void Index_WhenAvailableSizesContainsExpiredItems_ShouldReturnHttpNotFoundResult()
        {
            FashionProduct fashionProduct = null;
            FashionVariant fashionVariant = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {
                var sizes = new ItemCollection<string>() { "small", "medium" };

                fashionProduct = CreateFashionProduct();
                SetAvailableSizes(fashionProduct, sizes);

                // setup variant was expired.
                fashionVariant = CreateFashionVariant();
                fashionVariant.StartPublish = DateTime.UtcNow.AddDays(-17); 
                fashionVariant.StopPublish = DateTime.UtcNow.AddDays(-7);

                SetRelation(fashionProduct, fashionVariant);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(fashionProduct, fashionVariant.Code);
            }

            // Assert
            {
                Assert.IsInstanceOfType(actionResult, typeof(HttpNotFoundResult));
            }
        }

        [TestMethod]
        public void Index_WhenAvailableSizesContainsUnpublishItems_ShouldReturnHttpNotFoundResult()
        {
            FashionProduct fashionProduct = null;
            FashionVariant fashionVariant = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {
                var sizes = new ItemCollection<string>() { "small", "medium" };

                fashionProduct = CreateFashionProduct();
                SetAvailableSizes(fashionProduct, sizes);

                // setup variant with inactive
                fashionVariant = CreateFashionVariant();
                fashionVariant.IsPendingPublish = true;
                fashionVariant.Status = VersionStatus.CheckedIn;

                SetRelation(fashionProduct, fashionVariant);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            { 
                actionResult = productController.Index(fashionProduct, fashionVariant.Code);
            }

            // Assert
            {
                Assert.IsInstanceOfType(actionResult, typeof(HttpNotFoundResult));
            }
        }

        [TestMethod]
        public void Index_WhenAvailableSizesContainsItemUnavailabelInCurrentMarket_ShouldReturnHttpNotFoundResult()
        {
            FashionProduct fashionProduct = null;
            FashionVariant fashionVariant = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {
                var sizes = new ItemCollection<string>() { "small", "medium" };

                fashionProduct = CreateFashionProduct();
                SetAvailableSizes(fashionProduct, sizes);

                // setup variant unavailable in default market
                fashionVariant = CreateFashionVariant();
                fashionVariant.MarketFilter = new ItemCollection<string>() { "Default" };  

                SetRelation(fashionProduct, fashionVariant);
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(fashionProduct, fashionVariant.Code);
            }

            // Assert
            {
                Assert.IsInstanceOfType(actionResult, typeof(HttpNotFoundResult));
            }
        }

        [TestMethod]
        public void Index_WhenIsInEditModeAndHasNoVariation_ShouldReturnProductWithoutVariationView()
        {
            FashionProduct fashionProduct = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {
                _isInEditMode = true;
                fashionProduct = CreateFashionProduct();
                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(fashionProduct, "notexist");
            }

            // Assert
            {
                var model = (FashionProductViewModel)((ViewResultBase)actionResult).Model;

                model.Product.ShouldBeEquivalentTo(fashionProduct);
            }
        }

        [TestMethod]
        public void Index_WhenVariationCodeHasValue_ShouldSetColorsToTheAvailableColorsForTheVariationSize()
        {
            const string variationColorBlue = "blue";
            const string variationColorWhite = "white";
            FashionProduct fashionProduct = null;
            FashionVariant fashionVariantSmallBlue = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {
                var sizes = new ItemCollection<string>() { "small", "medium" };
                var colors = new ItemCollection<string>() { "red", variationColorBlue, "yellow", variationColorWhite, "green" };

                fashionProduct = CreateFashionProduct();
                SetAvailableSizes(fashionProduct, sizes);
                SetAvailableColors(fashionProduct, colors);

                fashionVariantSmallBlue = CreateFashionVariant("small", variationColorBlue);
                var fashionVariantSmallWhite = CreateFashionVariant("small", variationColorWhite);

                SetRelation(fashionProduct, new[] { fashionVariantSmallBlue, fashionVariantSmallWhite });
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(fashionProduct, fashionVariantSmallBlue.Code);
            }

            // Assert
            {
                var model = (FashionProductViewModel)((ViewResultBase)actionResult).Model;
                var expectedColors = String.Join(";", new[] { variationColorBlue, variationColorWhite });
                var modelColors = String.Join(";", model.Colors.Select(x => x.Value));

                Assert.AreEqual<string>(expectedColors, modelColors);
            }
        }

        [TestMethod]
        public void Index_WhenVariationCodeHasValue_ShouldSetSizesToTheAvailableSizesForTheVariationColor()
        {
            const string variationSizeMedium = "medium";
            const string variationSizeXlarge = "x-large";
            FashionProduct fashionProduct = null;
            FashionVariant fashionVariantMediumRed = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {
                var sizes = new ItemCollection<string>() { "small", variationSizeMedium, "large", variationSizeXlarge, "xx-large" };
                var colors = new ItemCollection<string>() { "red" };

                fashionProduct = CreateFashionProduct();
                SetAvailableSizes(fashionProduct, sizes);
                SetAvailableColors(fashionProduct, colors);

                fashionVariantMediumRed = CreateFashionVariant(variationSizeMedium, "red");
                var fashionVariantXlargeRed = CreateFashionVariant(variationSizeXlarge, "red");

                SetRelation(fashionProduct, new[] { fashionVariantMediumRed, fashionVariantXlargeRed });
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(fashionProduct, fashionVariantMediumRed.Code);
            }

            // Assert
            {
                var model = (FashionProductViewModel)((ViewResultBase)actionResult).Model;
                var expectedSizes = String.Join(";", new[] { variationSizeMedium, variationSizeXlarge });
                var modelSizes = String.Join(";", model.Sizes.Select(x => x.Value));

                Assert.AreEqual<string>(expectedSizes, modelSizes);
            }
        }

        [TestMethod]
        public void SelectVariant_WhenColorAndSizeHasValues_ShouldGetVariantWithSelectedColorAndSize()
        {
            FashionProduct fashionProduct = null;
            FashionVariant fashionVariantSmallGreen = null;
            FashionVariant fashionVariantSmallRed = null;
            FashionVariant fashionVariantMediumGreen = null;
            FashionVariant fashionVariantMediumRed = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {
                var sizes = new ItemCollection<string>() { "small", "medium" };
                var colors = new ItemCollection<string>() { "green", "red" };

                fashionProduct = CreateFashionProduct();
                SetAvailableSizes(fashionProduct, sizes);
                SetAvailableColors(fashionProduct, colors);

                fashionVariantSmallGreen = CreateFashionVariant("small", "green");
                fashionVariantSmallRed = CreateFashionVariant("small", "red");
                fashionVariantMediumGreen = CreateFashionVariant("medium", "green");
                fashionVariantMediumRed = CreateFashionVariant("medium", "red");
                
                SetRelation(fashionProduct, new[]
                {
                    fashionVariantSmallGreen,
                    fashionVariantSmallRed,
                    fashionVariantMediumGreen,
                    fashionVariantMediumRed,
                });

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.SelectVariant(fashionProduct, "red", "small");
            }

            // Assert
            {
                var selectedCode = ((RedirectToRouteResult) actionResult).RouteValues["variationCode"] as string;
                Assert.AreEqual<string>("redsmall", selectedCode);
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
        private bool _isInEditMode;

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

        [TestCleanup]
        public void Cleanup()
        {
            _isInEditMode = false;
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
                () => _preferredCulture,
                () => _isInEditMode);

            controller.ControllerContext = new ControllerContext(_mockHttpContextBase.Object, new RouteData(), controller);

            return controller;
        }

        private static FashionVariant CreateFashionVariant(string size, string color)
        {
            var fashionVariant = CreateFashionVariant(color + size);
            SetSize(fashionVariant, size);
            SetColor(fashionVariant, color);

            return fashionVariant;
        }

        private static FashionVariant CreateFashionVariant(string code = "myVariant")
        {
            var fashionVariant = new FashionVariant
            {
                ContentLink = new ContentReference(740),
                Code = code,
                IsDeleted = false,
                IsPendingPublish = false,
                Status = VersionStatus.Published,
                StartPublish = DateTime.UtcNow.AddDays(-7),
                StopPublish = DateTime.UtcNow.AddDays(7),
                MarketFilter = new ItemCollection<string>() {"USA"}
            };

            return fashionVariant;
        }

        private static FashionProduct CreateFashionProduct()
        {
            var fashionProduct = new FashionProduct
            {
                ContentLink = new ContentReference(741),
                Code = "myProduct",
                IsDeleted = false,
                IsPendingPublish = false,
                Status = VersionStatus.Published,
                StartPublish = DateTime.UtcNow.AddDays(-7),
                StopPublish = DateTime.UtcNow.AddDays(7),
                MarketFilter = new ItemCollection<string>() {"USA"}
            };

            SetAvailableColors(fashionProduct, new ItemCollection<string>());
            SetAvailableSizes(fashionProduct, new ItemCollection<string>());

            return fashionProduct;
        }

        private static void SetAvailableColors(FashionProduct product, ItemCollection<string> colors)
        {
            product.AvailableColors = colors;
        }

        private static void SetAvailableSizes(FashionProduct product, ItemCollection<string> sizes)
        {
            product.AvailableSizes = sizes;
        }

        private static void SetColor(FashionVariant fashionVariant, string color)
        {
            fashionVariant.Color = color;
        }

        private static void SetSize(FashionVariant fashionVariant, string size)
        {
            fashionVariant.Size = size;
        }

        private void SetRelation(IContent source, IContent target)
        {
            SetRelation(source, new[] {target});
        }

        private void SetRelation(IContent source, IEnumerable<IContent> targets)
        {
            SetRelation(source, targets.Select(x => new ProductVariation() { Source = source.ContentLink, Target = x.ContentLink }));

            SetGetItems(new[] { source.ContentLink }, new[] { source });
            SetGetItems(targets.Select(x => x.ContentLink), targets);
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

        private static void SetMediaCollection(IAssetContainer assetContainer, CommerceMedia media)
        {
            assetContainer.CommerceMediaCollection = new ItemCollection<CommerceMedia>() { media };
        }
    }
}
