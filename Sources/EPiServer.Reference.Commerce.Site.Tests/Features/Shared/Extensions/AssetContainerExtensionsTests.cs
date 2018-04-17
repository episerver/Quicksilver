using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Web.Routing;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Shared.Extensions
{
    public class AssetContainerExtensionsTests
    {
        [Fact]
        public void GetAssets_WhenCommerceMediaCollectionIsEmpty_ShouldReturnOneEmptyItem()
        {
            // Arrange
            _assetContainerMock
                .SetupGet(a => a.CommerceMediaCollection)
                .Returns(new ItemCollection<CommerceMedia>() { });

            // Act
            var result = AssetContainerExtensions.GetAssets<IContentMedia>(_assetContainerMock.Object, _contentLoaderMock.Object, _urlResolverMock.Object);

            // Assert
            Assert.Equal(String.Empty, result.Single());
        }

        [Fact]
        public void GetAssets_WhenCommerceMediaCollectionHasTwoEntries_ShouldReturnTwoItems()
        {
            // Arrange
            _assetContainerMock
                .SetupGet(a => a.CommerceMediaCollection)
                .Returns(new ItemCollection<CommerceMedia>() 
                { 
                    new CommerceMedia(), 
                    new CommerceMedia() 
                });
            
            // Act
            var result = AssetContainerExtensions.GetAssets<IContentMedia>(_assetContainerMock.Object, _contentLoaderMock.Object, _urlResolverMock.Object);

            // Assert
            Assert.Equal<int>(2, result.Count());
        }

        [Fact]
        public void GetAssets_WhenMediaTypeIsNotIContentMediaAndContentLinkIsEmpty_ShouldReturnOneEmptyItem()
        {
            // Arrange
            _assetContainerMock
                .SetupGet(a => a.CommerceMediaCollection)
                .Returns(new ItemCollection<CommerceMedia>() 
                { 
                    new CommerceMedia() { AssetLink = ContentReference.EmptyReference }, 
                    new CommerceMedia() { AssetLink = ContentReference.EmptyReference }
                });

            // Act
            var result = AssetContainerExtensions.GetAssets<IContentImage>(_assetContainerMock.Object, _contentLoaderMock.Object, _urlResolverMock.Object);

            // Assert
            Assert.Equal(String.Empty, result.Single());
        }

        [Fact]
        public void GetAssets_WhenMediaTypeIsNotIContentMediaAndContentLinkHasValue_ShouldNotFilterResult()
        {
            // Arrange
            _assetContainerMock
                .SetupGet(a => a.CommerceMediaCollection)
                .Returns(new ItemCollection<CommerceMedia>() 
                { 
                    new CommerceMedia() { AssetLink = new ContentReference(12) }, 
                    new CommerceMedia() { AssetLink = new ContentReference(13) }
                });

            // Act
            var result = AssetContainerExtensions.GetAssets<IContentImage>(_assetContainerMock.Object, _contentLoaderMock.Object, _urlResolverMock.Object);

            // Assert
            Assert.Equal<int>(2, result.Count());
        }

        [Fact]
        public void GetAssets_WhenMediaTypeIsNotIContentMediaAndContentCantBeResolved_ShouldReturnOneEmptyItem()
        {
            // Arrange
            _assetContainerMock
                .SetupGet(a => a.CommerceMediaCollection)
                .Returns(new ItemCollection<CommerceMedia>() 
                { 
                    new CommerceMedia() { AssetLink = new ContentReference(12) }, 
                    new CommerceMedia() { AssetLink = new ContentReference(13) }
                });

            IContent outValue;
            _contentLoaderMock.Setup(c => c.TryGet(It.IsAny<ContentReference>(), out outValue)).Returns(false);

            // Act
            var result = AssetContainerExtensions.GetAssets<IContentImage>(_assetContainerMock.Object, _contentLoaderMock.Object, _urlResolverMock.Object);

            // Assert
            Assert.Equal(String.Empty, result.Single());
        }

        Mock<IAssetContainer> _assetContainerMock;
        Mock<UrlResolver> _urlResolverMock;
        Mock<IContentLoader> _contentLoaderMock;


        public AssetContainerExtensionsTests()
        {
            _assetContainerMock = new Mock<IAssetContainer>();
            _urlResolverMock = new Mock<UrlResolver>();
            _contentLoaderMock = new Mock<IContentLoader>();

            IContent outValue;
            _contentLoaderMock.Setup(c => c.TryGet(It.IsAny<ContentReference>(), out outValue)).Returns(true);
        }
    }
}
