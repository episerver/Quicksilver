using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Reference.Commerce.Site.Features.Search.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Search.Models;
using Moq;
using System.Web.Mvc;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Search.Controllers
{

    public class CategoryPartialControllerTests
    {
        [Fact]
        public void Index_WhenCallingViewModelFactory_ShouldSetPageSizeToThree()
        {
            // Act
            var result = (PartialViewResult)_subject.Index(null);

            // Assert
            _viewModelFactoryMock.Verify(v => v.Create(It.IsAny<NodeContent>(), It.Is<FilterOptionFormModel>(f => f.PageSize == 3)));
        }

        [Fact]
        public void Index_WhenCallingViewModelFactory_ShouldSetPageToOne()
        {
            // Act
            var result = (PartialViewResult)_subject.Index(null);

            // Assert
            _viewModelFactoryMock.Verify(v => v.Create(It.IsAny<NodeContent>(), It.Is<FilterOptionFormModel>(f => f.Page == 1)));
        }

        [Fact]
        public void Index_WhenCallingViewModelFactory_ShouldSetFacetGroupsToEmptyList()
        {
            // Act
            var result = (PartialViewResult)_subject.Index(null);

            // Assert
            _viewModelFactoryMock.Verify(v => v.Create(It.IsAny<NodeContent>(), It.Is<FilterOptionFormModel>(f => f.FacetGroups.Count == 0)));
        }

        [Fact]
        public void Index_WhenCallingViewModelFactory_ShouldPassAlongNodeContent()
        {
            // Arrange
            var nodeContent = new NodeContent();

            // Act
            var result = (PartialViewResult)_subject.Index(nodeContent);

            // Assert
            _viewModelFactoryMock.Verify(v => v.Create(nodeContent, It.IsAny<FilterOptionFormModel>()));
        }

        CategoryPartialController _subject;
        Mock<SearchViewModelFactory> _viewModelFactoryMock;


        public CategoryPartialControllerTests()
        {
            _viewModelFactoryMock = new Mock<SearchViewModelFactory>(null, null);

            _viewModelFactoryMock
                .Setup(v => v.Create(It.IsAny<NodeContent>(), It.IsAny<FilterOptionFormModel>()))
                .Returns(new SearchViewModel<NodeContent>());

            _subject = new CategoryPartialController(_viewModelFactoryMock.Object);
        }
    }
}
