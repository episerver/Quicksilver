using EPiServer.Core;
using EPiServer.Recommendations.Tracking;
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

            Assert.IsAssignableFrom(typeof(ViewResultBase), result);
            Assert.IsType(typeof(FashionProductViewModel), (result as ViewResultBase).Model);
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
        public void Index_WhenSkipTrackingIsTrue_ShouldNotSendTracking()
        {
            var fashionProduct = new FashionProduct();

            _viewModelFactoryMock.Setup(x => x.Create(It.IsAny<FashionProduct>(), It.IsAny<string>()))
                .Returns(new FashionProductViewModel { Variant = new FashionVariant(), Product = fashionProduct });

            _recommendationServiceMock.Setup(x => x.SendProductTracking(It.IsAny<HttpContextBase>(), It.IsAny<string>(), It.IsAny<RetrieveRecommendationMode>())).Verifiable();

            CreateController().Index(fashionProduct, "notexist", false, true);

            _recommendationServiceMock.Verify(x => x.SendProductTracking(It.IsAny<HttpContextBase>(), It.IsAny<string>(), It.IsAny<RetrieveRecommendationMode>()), Times.Never);
        }

        [Fact]
        public void Index_WhenQuickViewIsTrue_And_SkipTrackingIsFalse_ShouldSendTrackingWithoutRetrieveRecommendations()
        {
            var fashionProduct = new FashionProduct();

            _viewModelFactoryMock.Setup(x => x.Create(It.IsAny<FashionProduct>(), It.IsAny<string>()))
                .Returns(new FashionProductViewModel { Variant = new FashionVariant(), Product = fashionProduct });

            _recommendationServiceMock.Setup(x => x.SendProductTracking(It.IsAny<HttpContextBase>(), It.IsAny<string>(), It.IsAny<RetrieveRecommendationMode>())).Verifiable();

            CreateController().Index(fashionProduct, "notexist", true, false);

            _recommendationServiceMock.Verify(x => x.SendProductTracking(It.IsAny<HttpContextBase>(), It.IsAny<string>(), RetrieveRecommendationMode.Disabled), Times.Once);
        }

        [Fact]
        public void Index_WhenQuickViewIsTrue_And_SkipTrackingIsTrue_ShouldNotSendTracking()
        {
            var fashionProduct = new FashionProduct();

            _viewModelFactoryMock.Setup(x => x.Create(It.IsAny<FashionProduct>(), It.IsAny<string>()))
                .Returns(new FashionProductViewModel { Variant = new FashionVariant(), Product = fashionProduct });

            _recommendationServiceMock.Setup(x => x.SendProductTracking(It.IsAny<HttpContextBase>(), It.IsAny<string>(), It.IsAny<RetrieveRecommendationMode>())).Verifiable();

            CreateController().Index(fashionProduct, "notexist", true, true);

            _recommendationServiceMock.Verify(x => x.SendProductTracking(It.IsAny<HttpContextBase>(), It.IsAny<string>(), It.IsAny<RetrieveRecommendationMode>()), Times.Never);
        }

        [Fact]
        public void Index_WhenQuickViewIsFalse_ShouldSendTrackingWithRetrieveRecommendations()
        {
            var fashionProduct = new FashionProduct();

            _viewModelFactoryMock.Setup(x => x.Create(It.IsAny<FashionProduct>(), It.IsAny<string>()))
                .Returns(new FashionProductViewModel { Variant = new FashionVariant(), Product = fashionProduct });

            _recommendationServiceMock.Setup(x => x.SendProductTracking(It.IsAny<HttpContextBase>(), It.IsAny<string>(), It.IsAny<RetrieveRecommendationMode>())).Verifiable();

            CreateController().Index(fashionProduct, "notexist", false, false);

            _recommendationServiceMock.Verify(x => x.SendProductTracking(It.IsAny<HttpContextBase>(), It.IsAny<string>(), RetrieveRecommendationMode.Enabled), Times.Once);
        }

        [Fact]
        public void SelectVariant_WhenFoundBySizeOrColor_ShouldReturnCorrectRouteValue()
        {
            _viewModelFactoryMock.Setup(x => x.SelectVariant(It.IsAny<FashionProduct>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new FashionVariant { Code = "redsmall" });

            var result = CreateController().SelectVariant(new FashionProduct(), "red", "small");

            var selectedCode = ((RedirectToRouteResult)result).RouteValues["entryCode"] as string;

            Assert.Equal<string>("redsmall", selectedCode);
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

            _viewModelFactoryMock = new Mock<CatalogEntryViewModelFactory>(null, null, null, null, null, null, null, null, null);
            _recommendationServiceMock = new Mock<IRecommendationService>();

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
