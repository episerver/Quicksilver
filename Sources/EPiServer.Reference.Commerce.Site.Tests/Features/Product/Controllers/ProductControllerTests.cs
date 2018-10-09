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
        public void Index_ShouldReturnCorrectTypes()
        {
            _viewModelFactoryMock.Setup(x => x.Create(It.IsAny<FashionProduct>(), It.IsAny<string>()))
                .Returns(new FashionProductViewModel { Variant = new FashionVariant() });

            var result = CreateController().Index(new FashionProduct(), "code");

            Assert.IsAssignableFrom<ViewResultBase>(result);
            Assert.IsType<FashionProductViewModel>((result as ViewResultBase).Model);
        }

        [Fact]
        public void Index_WhenIsInEditModeAndHasNoVariation_ShouldReturnProductWithoutVariationView()
        {
            _isInEditMode = true;
            var fashionProduct = new FashionProduct();

            _viewModelFactoryMock.Setup(x => x.Create(It.IsAny<FashionProduct>(), It.IsAny<string>()))
                .Returns(new FashionProductViewModel { Variant = null, Product = fashionProduct });

            var actionResult = CreateController().Index(fashionProduct, "notexist");

            var viewModel = (FashionProductViewModel)((ViewResultBase)actionResult).Model;

            Assert.Equal<FashionProduct>(fashionProduct, viewModel.Product);
            Assert.Null(viewModel.Variant);
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
                .ReturnsAsync((TrackingResponseData)null);

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
            var controller = new ProductController(() => _isInEditMode, _viewModelFactoryMock.Object);
            controller.ControllerContext = new ControllerContext(_httpContextBaseMock.Object, new RouteData(), controller);

            return controller;
        }
    }
}
