using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Product.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Product.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Product.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Services;
using Mediachase.Commerce.Catalog;
using Moq;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using EPiServer.Tracking.Commerce.Data;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Product.Controllers
{
    public class ProductControllerTests : IDisposable
    {
        [Fact]
        public async void Index_ShouldReturnCorrectTypes()
        {
            _viewModelFactoryMock.Setup(x => x.Create(It.IsAny<FashionProduct>(), It.IsAny<string>()))
                .Returns(new FashionProductViewModel { Variant = new FashionVariant() });

            var result = await CreateController().Index(new FashionProduct(), "code");

            Assert.IsAssignableFrom<ViewResultBase>(result);
            Assert.IsType<FashionProductViewModel>((result as ViewResultBase).Model);
        }

        [Fact]
        public async void Index_WhenIsInEditModeAndHasNoVariation_ShouldReturnProductWithoutVariationView()
        {
            _isInEditMode = true;
            var fashionProduct = new FashionProduct();

            _viewModelFactoryMock.Setup(x => x.Create(It.IsAny<FashionProduct>(), It.IsAny<string>()))
                .Returns(new FashionProductViewModel { Variant = null, Product = fashionProduct });

            var actionResult = await CreateController().Index(fashionProduct, "notexist");

            var viewModel = (FashionProductViewModel)((ViewResultBase)actionResult).Model;

            Assert.Equal<FashionProduct>(fashionProduct, viewModel.Product);
            Assert.Null(viewModel.Variant);
        }

        [Fact]
        public async void Index_WhenSkipTrackingIsTrue_ShouldNotSendTracking()
        {
            var fashionProduct = new FashionProduct();

            _viewModelFactoryMock.Setup(x => x.Create(It.IsAny<FashionProduct>(), It.IsAny<string>()))
                .Returns(new FashionProductViewModel { Variant = new FashionVariant(), Product = fashionProduct });

            await CreateController().Index(fashionProduct, null, false, true);

            _recommendationServiceMock.Verify(x => x.TrackProductAsync(It.IsAny<HttpContextBase>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async void Index_WhenQuickViewIsTrue_And_SkipTrackingIsFalse_ShouldSendTrackingWithoutRetrieveRecommendations()
        {
            var fashionProduct = new FashionProduct();

            _viewModelFactoryMock.Setup(x => x.Create(It.IsAny<FashionProduct>(), It.IsAny<string>()))
                .Returns(new FashionProductViewModel { Variant = new FashionVariant(), Product = fashionProduct });

            await CreateController().Index(fashionProduct, null, true, false);

            _recommendationServiceMock.Verify(x => x.TrackProductAsync(It.IsAny<HttpContextBase>(), It.IsAny<string>(), true), Times.Once);
        }

        [Fact]
        public async void Index_WhenQuickViewIsTrue_And_SkipTrackingIsTrue_ShouldNotSendTracking()
        {
            var fashionProduct = new FashionProduct();

            _viewModelFactoryMock.Setup(x => x.Create(It.IsAny<FashionProduct>(), It.IsAny<string>()))
                .Returns(new FashionProductViewModel { Variant = new FashionVariant(), Product = fashionProduct });

            await CreateController().Index(fashionProduct, null, true, true);

            _recommendationServiceMock.Verify(x => x.TrackProductAsync(It.IsAny<HttpContextBase>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async void Index_WhenQuickViewIsFalse_ShouldSendTrackingWithRetrieveRecommendations()
        {
            var fashionProduct = new FashionProduct();

            _viewModelFactoryMock.Setup(x => x.Create(It.IsAny<FashionProduct>(), It.IsAny<string>()))
                .Returns(new FashionProductViewModel { Variant = new FashionVariant(), Product = fashionProduct });

            await CreateController().Index(fashionProduct, null, false, false);

            _recommendationServiceMock.Verify(x => x.TrackProductAsync(It.IsAny<HttpContextBase>(), It.IsAny<string>(), false), Times.Once);
        }

        [Fact]
        public void SelectVariant_WhenFoundBySizeOrColor_ShouldReturnCorrectRouteValue()
        {
            _viewModelFactoryMock.Setup(x => x.SelectVariant(It.IsAny<FashionProduct>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new FashionVariant { Code = "redsmall" });

            var result = CreateController().SelectVariant(new FashionProduct(), "red", "small");

            var selectedCode = ((RedirectToRouteResult)result).RouteValues["entryCode"] as string;

            Assert.Equal("redsmall", selectedCode);
        }

        [Fact]
        public void SelectVariant_ShouldPassSkipTrackingAsTrue()
        {
            _viewModelFactoryMock.Setup(x => x.SelectVariant(It.IsAny<FashionProduct>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new FashionVariant { Code = "redsmall" });

            var result = CreateController().SelectVariant(new FashionProduct(), "red", "small");

            var skipTracking = ((RedirectToRouteResult)result).RouteValues["skipTracking"];

            Assert.True((bool)skipTracking);
        }

        [Fact]
        public void SelectVariant_WhenCanNotFoundBySizeOrColor_ShouldReturnHttpNotFoundResult()
        {
            _viewModelFactoryMock.Setup(x => x.SelectVariant(It.IsAny<FashionProduct>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => null);

            var result = CreateController().SelectVariant(new FashionProduct(), "red", "small");

            Assert.IsType<HttpNotFoundResult>(result);
        }

        private readonly Mock<CatalogEntryViewModelFactory> _viewModelFactoryMock;
        private readonly Mock<HttpContextBase> _httpContextBaseMock;
        private readonly Mock<IRecommendationService> _recommendationServiceMock;
        private readonly Mock<ReferenceConverter> _referenceConverterMock;
        private bool _isInEditMode;

        public ProductControllerTests()
        {
            var request = new Mock<HttpRequestBase>();
            request.SetupGet(x => x.Headers).Returns(
                new System.Net.WebHeaderCollection {
                {"X-Requested-With", "XMLHttpRequest"}
            });

            _httpContextBaseMock = new Mock<HttpContextBase>();
            _httpContextBaseMock.SetupGet(x => x.Request).Returns(request.Object);

            _viewModelFactoryMock = new Mock<CatalogEntryViewModelFactory>(null, null, null, null);
            _recommendationServiceMock = new Mock<IRecommendationService>();
            _recommendationServiceMock
                .Setup(m => m.TrackProductAsync(It.IsAny<HttpContextBase>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync((TrackingResponseData) null);

            _referenceConverterMock = new Mock<ReferenceConverter>(null, null);
            _referenceConverterMock.Setup(x => x.GetContentLink(It.IsAny<string>()))
                .Returns(() => new ContentReference(1));
        }

        public void Dispose()
        {
            _isInEditMode = false;
        }

        private ProductController CreateController()
        {
            var controller = new ProductController(() => _isInEditMode, _viewModelFactoryMock.Object, _recommendationServiceMock.Object, _referenceConverterMock.Object);
            controller.ControllerContext = new ControllerContext(_httpContextBaseMock.Object, new RouteData(), controller);

            return controller;
        }
    }
}
