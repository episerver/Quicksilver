using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Reference.Commerce.Site.Features.Product.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Product.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Product.ViewModels;
using Moq;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Services;
using Mediachase.Commerce.Catalog;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Product.Controllers
{
    public class PackageControllerTests
    {
        [Fact]
        public void Index_ShouldReturnCorrectTypes()
        {
            var result = CreateController().Index(new FashionPackage());
            Assert.IsAssignableFrom<ViewResultBase>(result);
            Assert.IsType<FashionPackageViewModel>(((ViewResultBase) result).Model);
        }

        [Fact]
        public void Index_WhenInEditModeAndHasNoEntries_ShouldReturnViewModelWithNoEntries()
        {
            _isInEditMode = true;
            var result = CreateController().Index(new FashionPackage());
            var viewModel = (FashionPackageViewModel)((ViewResultBase)result).Model;
            Assert.Empty(viewModel.Entries);
        }

        [Fact]
        public void Index_WhenInEditModeAndHasEntries_ShouldReturnViewModelWithEntries()
        {
            _catalogEntryViewModelFactoryMock.Setup(x => x.Create(It.IsAny<FashionPackage>()))
               .Returns(() => new FashionPackageViewModel { Entries = new [] {new VariationContent(), } });

            _isInEditMode = true;
            var result = CreateController().Index(new FashionPackage());
            var viewModel = (FashionPackageViewModel)((ViewResultBase)result).Model;
            Assert.NotEmpty(viewModel.Entries);
        }

        private bool _isInEditMode = false;
        private readonly Mock<CatalogEntryViewModelFactory> _catalogEntryViewModelFactoryMock;
        private readonly Mock<IRecommendationService> _recomendationserviceMock;
        public PackageControllerTests()
        {
            _catalogEntryViewModelFactoryMock = new Mock<CatalogEntryViewModelFactory>(null, null, null, null);
            _recomendationserviceMock = new Mock<IRecommendationService>();
            _catalogEntryViewModelFactoryMock.Setup(x => x.Create(It.IsAny<FashionPackage>()))
                .Returns(() => new FashionPackageViewModel { Entries = Enumerable.Empty<EntryContentBase>() });
        }

        private PackageController CreateController()
        {
            var request = new Mock<HttpRequestBase>();
            var _httpContextBaseMock = new Mock<HttpContextBase>();
            _httpContextBaseMock.SetupGet(x => x.Request).Returns(request.Object);

            var controller = new PackageController(() => _isInEditMode, _catalogEntryViewModelFactoryMock.Object);
            controller.ControllerContext = new ControllerContext(_httpContextBaseMock.Object, new RouteData(), controller);

            return controller;
        }
    }
}
