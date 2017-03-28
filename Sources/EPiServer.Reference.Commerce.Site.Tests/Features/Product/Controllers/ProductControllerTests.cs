using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.Core;
using EPiServer.Filters;
using EPiServer.Globalization;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Product.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Product.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Product.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Web.Routing;
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
                .Returns(new FashionProductViewModel {Variant = null, Product = fashionProduct});

            var actionResult = CreateController().Index(fashionProduct, "notexist");

            var viewModel = (FashionProductViewModel) ((ViewResultBase) actionResult).Model;

            Assert.Equal<FashionProduct>(fashionProduct, viewModel.Product);
            Assert.Null(viewModel.Variant);
        }

        [Fact]
        public void SelectVariant_WhenFoundBySizeOrColor_ShouldReturnCorrectRouteValue()
        {
            _viewModelFactoryMock.Setup(x => x.SelectVariant(It.IsAny<FashionProduct>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new FashionVariant {Code = "redsmall" });
           
            var result = CreateController().SelectVariant(new FashionProduct(), "red", "small");

            var selectedCode = ((RedirectToRouteResult)result).RouteValues["variationCode"] as string;

            Assert.Equal<string>("redsmall", selectedCode);
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

            _viewModelFactoryMock = new Mock<CatalogEntryViewModelFactory>(null,null,null,null,null,null,null,null,null,null);
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
