using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.Core;
using EPiServer.Filters;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Product.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Product.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Web.Routing;
using FluentAssertions;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Pricing;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Xunit;


namespace EPiServer.Reference.Commerce.Site.Tests.Features.Product.Controllers
{
    public class ProductControllerTests : IDisposable
    {
        [Fact]
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
                Assert.IsType(typeof(HttpNotFoundResult), actionResult);
            }
        }

        [Fact]
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
                Assert.IsType(typeof(HttpNotFoundResult), actionResult);
            }
        }

        [Fact]
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
                Assert.IsType(typeof(HttpNotFoundResult), actionResult);
            }
        }

        [Fact]
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
                Assert.IsType(typeof(HttpNotFoundResult), actionResult);
            }
        }

        [Fact]
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
                Assert.Equal<FashionVariant>(fashionVariant, model.Variation);
            }
        }

        [Fact]
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
                Assert.Equal<FashionProduct>(fashionProduct, model.Product);
            }
        }

        [Fact]
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
                Assert.Equal<Money>(mockDefaultPrice.Object.UnitPrice, model.ListingPrice);
            }
        }

        [Fact]
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
                Assert.Equal<Money>(mockDiscountPrice.Object.UnitPrice, model.DiscountedPrice.Value);
            }
        }

        [Fact]
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
                Assert.Equal<string>(color, model.Color);
            }
        }

        [Fact]
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
                Assert.Equal<string>(size, model.Size);
            }
        }

        [Fact]
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
                Assert.Equal<string>(string.Empty, model.Images.Single());
            }
        }

        [Fact]
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
                Assert.Equal<string>(imageLink, model.Images.Single());
            }
        }

        [Fact]
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
                Assert.Equal<int>(0, model.Colors.Count());
            }
        }

        [Fact]
        public void Index_WhenAvailableColorsContainsItems_ShouldSetTextToItemValue()
        {
            FashionProduct fashionProduct = null;
            FashionVariant fashionVariant1 = null;
            FashionVariant fashionVariant2 = null;
            FashionVariant fashionVariant3 = null;
            ProductController productController = null;
            ItemCollection<string> colors = null;
            ActionResult actionResult = null;

            // Setup
            {
                colors = new ItemCollection<string>() { "green", "red" };

                fashionProduct = CreateFashionProduct();
                SetAvailableColors(fashionProduct, colors);
                SetAvailableSizes(fashionProduct, new ItemCollection<string> { "small", "medium" });

                fashionVariant1 = CreateFashionVariant("small", "green");
                fashionVariant2 = CreateFashionVariant("medium", "red");
                fashionVariant3 = CreateFashionVariant("medium", "green");

                SetRelation(fashionProduct, new[] { fashionVariant1, fashionVariant2, fashionVariant3 });
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(fashionProduct, fashionVariant1.Code);
            }

            // Assert
            {
                var model = (FashionProductViewModel)((ViewResultBase)actionResult).Model;
                var expectedColors = String.Join(";", colors);
                var modelColorTexts = String.Join(";", model.Colors.Select(x => x.Text));

                Assert.Equal<string>(expectedColors, modelColorTexts);
            }
        }

        [Fact]
        public void Index_WhenAvailableColorsContainsItems_ShouldSetValueToItemValue()
        {
            FashionProduct fashionProduct = null;
            FashionVariant fashionVariant1 = null;
            FashionVariant fashionVariant2 = null;
            FashionVariant fashionVariant3 = null;
            ProductController productController = null;
            ItemCollection<string> colors = null;
            ActionResult actionResult = null;

            // Setup
            {
                colors = new ItemCollection<string>() { "green", "red" };

                fashionProduct = CreateFashionProduct();
                SetAvailableColors(fashionProduct, colors);
                SetAvailableSizes(fashionProduct, new ItemCollection<string> { "small", "medium" });

                fashionVariant1 = CreateFashionVariant("small", "green");
                fashionVariant2 = CreateFashionVariant("medium", "red");
                fashionVariant3 = CreateFashionVariant("medium", "green");

                SetRelation(fashionProduct, new[] { fashionVariant1, fashionVariant2, fashionVariant3 });
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(fashionProduct, fashionVariant1.Code);
            }

            // Assert
            {
                var model = (FashionProductViewModel)((ViewResultBase)actionResult).Model;
                var expectedColors = String.Join(";", colors);
                var modelColorValues = String.Join(";", model.Colors.Select(x => x.Value));

                Assert.Equal<string>(expectedColors, modelColorValues);
            }
        }

        [Fact]
        public void Index_WhenAvailableColorsContainsItems_ShouldSetSelectedToFalse()
        {
            FashionProduct fashionProduct = null;
            FashionVariant fashionVariant1 = null;
            FashionVariant fashionVariant2 = null;
            FashionVariant fashionVariant3 = null;
            ProductController productController = null;
            ActionResult actionResult = null;

            // Setup
            {

                fashionProduct = CreateFashionProduct();
                SetAvailableColors(fashionProduct, new ItemCollection<string>() { "green", "red" });
                SetAvailableSizes(fashionProduct, new ItemCollection<string> { "small", "medium" });

                fashionVariant1 = CreateFashionVariant("small", "green");
                fashionVariant2 = CreateFashionVariant("medium", "red");
                fashionVariant3 = CreateFashionVariant("medium", "green");

                SetRelation(fashionProduct, new[] { fashionVariant1, fashionVariant2, fashionVariant3 });
                MockPrices();

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.Index(fashionProduct, fashionVariant1.Code);
            }

            // Assert
            {
                var model = (FashionProductViewModel)((ViewResultBase)actionResult).Model;
                var expectedColors = String.Join(";", new[] { false, false });
                var modelColorsSelected = String.Join(";", model.Colors.Select(x => x.Selected));

                Assert.Equal<string>(expectedColors, modelColorsSelected);
            }
        }

        [Fact]
        public void Index_WhenAvailableSizesContainsItems_ShouldSetTextToItemValue()
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
                var modelSizeTexts = String.Join(";", model.Sizes.Select(x => x.Text));

                Assert.Equal<string>(expectedSizes, modelSizeTexts);
            }
        }

        [Fact]
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

                Assert.Equal<string>(expectedSizes, modelSizeValues);
            }
        }

        [Fact]
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

                Assert.Equal<string>(expectedSizes, modelSizesSelected);
            }
        }

        [Fact]
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
                Assert.IsType(typeof(HttpNotFoundResult), actionResult);
            }
        }

        [Fact]
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
                Assert.IsType(typeof(HttpNotFoundResult), actionResult);
            }
        }

        [Fact]
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
                Assert.IsType(typeof(HttpNotFoundResult), actionResult);
            }
        }

        [Fact]
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
                Assert.IsType(typeof(HttpNotFoundResult), actionResult);
            }
        }

        [Fact]
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

        [Fact]
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

                Assert.Equal<string>(expectedColors, modelColors);
            }
        }

        [Fact]
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

                Assert.Equal<string>(expectedSizes, modelSizes);
            }
        }

        [Fact]
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
                var selectedCode = ((RedirectToRouteResult)actionResult).RouteValues["variationCode"] as string;
                Assert.Equal<string>("redsmall", selectedCode);
            }
        }

        [Fact]
        public void SelectVariant_WhenCanNotFoundBySize_ShouldTryGetVariantWithSelectedColorOnly()
        {
            FashionProduct fashionProduct = null;
            FashionVariant fashionVariantSmallGreen = null;
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
                fashionVariantMediumRed = CreateFashionVariant("medium", "red");

                SetRelation(fashionProduct, new[]
                {
                    fashionVariantSmallGreen,
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
                var selectedCode = ((RedirectToRouteResult)actionResult).RouteValues["variationCode"] as string;
                Assert.Equal<string>("redmedium", selectedCode);
            }
        }

        [Fact]
        public void SelectVariant_WhenCanNotFoundBySizeOrColor_ShouldReturnHttpNotFoundResult()
        {
            FashionProduct fashionProduct = null;
            FashionVariant fashionVariantSmallGreen = null;
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
                fashionVariantMediumRed = CreateFashionVariant("medium", "red");

                SetRelation(fashionProduct, new[]
                {
                    fashionVariantSmallGreen,
                    fashionVariantMediumRed,
                });

                productController = CreateController();
            }

            // Execute
            {
                actionResult = productController.SelectVariant(fashionProduct, "yellow", "small");
            }

            // Assert
            {
                Assert.IsType<HttpNotFoundResult>(actionResult);
            }
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
        private readonly Currency _defaultCurrency;
        private readonly CultureInfo _preferredCulture;
        private bool _isInEditMode;

        public ProductControllerTests()
        {
            _defaultCurrency = Currency.USD;
            _preferredCulture = CultureInfo.GetCultureInfo("en");

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

            SetGetItems(Enumerable.Empty<ContentReference>(), Enumerable.Empty<IContent>());
            SetDefaultCurrency(null);
        }

        public void Dispose()
        {
            _isInEditMode = false;
        }

        private ProductController CreateController()
        {
            var controller = new ProductController(
                _promotionServiceMock.Object,
                _contentLoaderMock.Object,
                _priceServiceMock.Object,
                _currentMarketMock.Object,
                _currencyserviceMock.Object,
                _relationRepositoryMock.Object,
                _appContextFacadeMock.Object,
                _urlResolverMock.Object,
                _filterPublished,
                () => _preferredCulture,
                () => _isInEditMode);

            controller.ControllerContext = new ControllerContext(_httpContextBaseMock.Object, new RouteData(), controller);

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
                MarketFilter = new ItemCollection<string>() { "USA" }
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
                MarketFilter = new ItemCollection<string>() { "USA" }
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
            _contentLoaderMock.Setup(x => x.GetItems(setup, _preferredCulture)).Returns(result);
        }

        private void SetDefaultCurrency(string currency)
        {
            _cookieServiceMock.Setup(x => x.Get("Currency")).Returns(currency);
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

        private CommerceMedia CreateImageMedia(ContentReference contentLink, string imageLink)
        {
            IContentImage contentImage;

            var imageMedia = new CommerceMedia { AssetLink = contentLink };
            _contentLoaderMock.Setup(x => x.TryGet(imageMedia.AssetLink, out contentImage)).Returns(true);
            _urlResolverMock.Setup(x => x.GetUrl(imageMedia.AssetLink)).Returns(imageLink);

            return imageMedia;
        }

        private static void SetMediaCollection(IAssetContainer assetContainer, CommerceMedia media)
        {
            assetContainer.CommerceMediaCollection = new ItemCollection<CommerceMedia>() { media };
        }
    }
}
