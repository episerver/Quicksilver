using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Shared.Extensions
{
    [TestClass]
    public class ContentLoaderExtensionsTests
    {
        [TestMethod]
        public void GetFirstChild_WhenNoChildren_ShouldReturnNull()
        {
            // Arrange
            _contentLoaderMock
                .Setup(c => c.GetChildren<IContentData>(It.IsAny<ContentReference>()))
                .Returns(Enumerable.Empty<IContentData>());

            // Act
            var result = ContentLoaderExtensions.GetFirstChild<IContentData>(_contentLoaderMock.Object, null);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetFirstChild_WhenOneChild_ShouldReturnThatChild()
        {
            // Arrange
            var child = new Mock<IContentData>().Object;
            _contentLoaderMock
                .Setup(c => c.GetChildren<IContentData>(It.IsAny<ContentReference>()))
                .Returns(new[] { child });

            // Act
            var result = ContentLoaderExtensions.GetFirstChild<IContentData>(_contentLoaderMock.Object, null);

            // Assert
            Assert.AreSame(child, result);
        }

        [TestMethod]
        public void GetFirstChild_WhenMultipleChildren_ShouldReturnFirst()
        {
            // Arrange
            var firstChild = new Mock<IContentData>().Object;
            var secondChild = new Mock<IContentData>().Object;
            _contentLoaderMock
                .Setup(c => c.GetChildren<IContentData>(It.IsAny<ContentReference>()))
                .Returns(new[] { firstChild, secondChild });

            // Act
            var result = ContentLoaderExtensions.GetFirstChild<IContentData>(_contentLoaderMock.Object, null);

            // Assert
            Assert.AreSame(firstChild, result);
        }

        Mock<IContentLoader> _contentLoaderMock;

        [TestInitialize]
        public void Setup()
        {
            _contentLoaderMock = new Mock<IContentLoader>();
        }
    }
}
