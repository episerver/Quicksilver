using System.Linq;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Web.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Shared.Extensions
{
    [TestClass]
    public class ContentExtensionsTests
    {
        [TestMethod]
        public void GetUrl_WhenVariationHasNoCode_ShouldReturnBaseUrl()
        {
            var variant = new VariationContent();

            var result = ContentExtensions.GetUrl(variant, _linkRepositoryMock.Object, _urlResolverMock.Object);

            Assert.AreEqual<string>(_url, result);
        }

        [TestMethod]
        public void GetUrl_WhenVariationHasCode_ShouldReturnUrlWithQuery()
        {
            var variant = new VariationContent {Code = "code"};

            var result = ContentExtensions.GetUrl(variant, _linkRepositoryMock.Object, _urlResolverMock.Object);

            Assert.AreEqual<string>(_url + "?variationCode=" + variant.Code, result);
        }

        [TestMethod]
        public void GetUrl_WhenNoRelationExists_ShouldReturnEmptyString()
        {
            _linkRepositoryMock
                .Setup(x => x.GetRelationsByTarget<ProductVariation>(It.IsAny<ContentReference>()))
                .Returns(Enumerable.Empty<ProductVariation>());

            var variant = new VariationContent();

            var result = ContentExtensions.GetUrl(variant, _linkRepositoryMock.Object, _urlResolverMock.Object);

            Assert.AreEqual<string>(string.Empty, result);
        }

        private Mock<ILinksRepository> _linkRepositoryMock;
        private Mock<UrlResolver> _urlResolverMock;
        private string _url;

        [TestInitialize]
        public void Setup()
        {
            _linkRepositoryMock = new Mock<ILinksRepository>();
            _linkRepositoryMock.Setup(x => x.GetRelationsByTarget<ProductVariation>(It.IsAny<ContentReference>()))
                .Returns(new[] { new ProductVariation() { Source = new ContentReference(1) } });

            _url = "http://domain.com/";
            _urlResolverMock = new Mock<UrlResolver>();
            _urlResolverMock.Setup(x => x.GetUrl(It.IsAny<ContentReference>())).Returns(_url);
        }
    }
}
