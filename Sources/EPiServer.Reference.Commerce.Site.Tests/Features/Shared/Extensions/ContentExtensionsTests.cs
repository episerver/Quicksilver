using System.Linq;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Web.Routing;
using Moq;
using Xunit;


namespace EPiServer.Reference.Commerce.Site.Tests.Features.Shared.Extensions
{
    public class ContentExtensionsTests
    {
        [Fact]
        public void GetUrl_WhenVariationHasNoCode_ShouldReturnBaseUrl()
        {
            var variant = new VariationContent();

            var result = ContentExtensions.GetUrl(variant, _relationRepositoryMock.Object, _urlResolverMock.Object);

            Assert.Equal(_url, result);
        }

        [Fact]
        public void GetUrl_WhenVariationHasCode_ShouldReturnUrlWithQuery()
        {
            var variant = new VariationContent {Code = "code"};

            var result = ContentExtensions.GetUrl(variant, _relationRepositoryMock.Object, _urlResolverMock.Object);

            Assert.Equal(_url + "?variationCode=" + variant.Code, result);
        }

        [Fact]
        public void GetUrl_WhenNoRelationExists_ShouldReturnEmptyString()
        {
            _relationRepositoryMock
                .Setup(x => x.GetParents<ProductVariation>(It.IsAny<ContentReference>()))
                .Returns(Enumerable.Empty<ProductVariation>());

            var variant = new VariationContent();

            var result = ContentExtensions.GetUrl(variant, _relationRepositoryMock.Object, _urlResolverMock.Object);

            Assert.Equal(string.Empty, result);
        }

        private Mock<IRelationRepository> _relationRepositoryMock;
        private Mock<UrlResolver> _urlResolverMock;
        private string _url;


        public ContentExtensionsTests()
        {
            _relationRepositoryMock = new Mock<IRelationRepository>();
            _relationRepositoryMock.Setup(x => x.GetParents<ProductVariation>(It.IsAny<ContentReference>()))
                .Returns(new[] {new ProductVariation {Parent = new ContentReference(1)}});

            _url = "http://domain.com/";
            _urlResolverMock = new Mock<UrlResolver>();
            _urlResolverMock.Setup(x => x.GetUrl(It.IsAny<ContentReference>())).Returns(_url);
        }
    }
}
