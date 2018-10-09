using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Services;
using EPiServer.Reference.Commerce.Site.Features.Search.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Search.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Search.ViewModels;
using Mediachase.Commerce.Catalog;
using Moq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Search.Controllers
{
    public class CategoryControllerTests
    {
        [Fact]
        public void Index_ShouldReturnViewResult()
        {
            // Act
            var result = _subject.Index(null, null);

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Facet_ShouldReturnPartialView()
        {
            // Act
            var result = _subject.Facet(null, null);

            // Assert
            Assert.IsType<PartialViewResult>(result);
        }

        [Fact]
        public void Index_WhenPassingFashionNode_ShouldPassItOnToFactory()
        {
            // Arrange
            var fashionNode = new FashionNode();

            // Act
            _subject.Index(fashionNode, null);

            // Assert
            _viewModelFactoryMock.Verify(v => v.Create(fashionNode, It.IsAny<FilterOptionViewModel>()));
        }

        [Fact]
        public void Index_WhenPassingFormModel_ShouldPassItOnToFactory()
        {
            // Arrange
            var formModel = new FilterOptionViewModel();

            // Act
            _subject.Index(null, formModel);

            // Assert
            _viewModelFactoryMock.Verify(v => v.Create(It.IsAny<FashionNode>(), formModel));
        }

        private readonly CategoryController _subject;
        private readonly Mock<SearchViewModelFactory> _viewModelFactoryMock;
        private readonly Mock<HttpRequestBase> _httpRequestMock;
        Mock<IRecommendationService> _recommendationServiceMock;

        public CategoryControllerTests()
        {
            var referenceConverterMock = new Mock<ReferenceConverter>(null, null);
            referenceConverterMock.Setup(x => x.GetContentLink(It.IsAny<string>()))
                .Returns(() => new ContentReference(1));
            _viewModelFactoryMock = new Mock<SearchViewModelFactory>(null, null);
            _httpRequestMock = new Mock<HttpRequestBase>();
            _recommendationServiceMock = new Mock<IRecommendationService>();

            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Request).Returns(_httpRequestMock.Object);

            _subject = new CategoryController(
                _viewModelFactoryMock.Object);
            _subject.ControllerContext = new ControllerContext(context.Object, new RouteData(), _subject);
        }
    }
}