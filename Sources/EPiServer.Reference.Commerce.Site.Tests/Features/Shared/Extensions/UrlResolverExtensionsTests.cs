using EPiServer.Core;
using EPiServer.Web.Routing;
using Moq;
using System;
using System.Web;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Shared.Extensions
{
    public class UrlResolverExtensionsTests
    {
        [Fact]
        public void UrlResolverExtensions_WhenContentLinkIsNull_ShouldReturnPathAndQueryFromRequestUrlReferrer()
        {
            var expectedUrlReferrer = new Uri("https://github.com/episerver/Quicksilver");
            _httpRequestBaseMock.Setup(x => x.UrlReferrer).Returns(expectedUrlReferrer);

            var result = Site.Features.Shared.Extensions.UrlResolverExtensions.GetUrl(_urlResolverMock.Object, _httpRequestBaseMock.Object, null, "en");
            Assert.Equal(expectedUrlReferrer.PathAndQuery, result);
        }

        [Fact]
        public void UrlResolverExtensions_WhenContentLinkIsNullAndUrlReferrerOnRequestIsNull_ShouldReturnSlash()
        {
            _httpRequestBaseMock.Setup(x => x.UrlReferrer).Returns((Uri)null);

            var result = Site.Features.Shared.Extensions.UrlResolverExtensions.GetUrl(_urlResolverMock.Object, _httpRequestBaseMock.Object, null, "en");
            Assert.Equal("/", result);
        }

        [Fact]
        public void UrlResolverExtensions_WhenContentLinkIsEmpty_ShouldReturnPathAndQueryFromRequestUrlReferrer()
        {
            var expectedUrlReferrer = new Uri("https://github.com/episerver/Quicksilver");
            _httpRequestBaseMock.Setup(x => x.UrlReferrer).Returns(expectedUrlReferrer);

            var result = Site.Features.Shared.Extensions.UrlResolverExtensions.GetUrl(_urlResolverMock.Object, _httpRequestBaseMock.Object, ContentReference.EmptyReference, "en");
            Assert.Equal(expectedUrlReferrer.PathAndQuery, result);
        }

        [Fact]
        public void UrlResolverExtensions_WhenContentLinkIsSet_ShouldReturnResultFromGetUrlOnUrlResolver()
        {
            const string expectedUrl = "https://github.com/episerver/Quicksilver";

            var contentLink = new ContentReference(11);
            const string language = "en";

            _urlResolverMock.Setup(x => x.GetUrl(contentLink, language)).Returns(expectedUrl);

            var result = Site.Features.Shared.Extensions.UrlResolverExtensions.GetUrl(_urlResolverMock.Object, _httpRequestBaseMock.Object, contentLink, language);
            Assert.Equal(expectedUrl, result);
        }

        private Mock<UrlResolver> _urlResolverMock;
        private Mock<HttpRequestBase> _httpRequestBaseMock;

        public UrlResolverExtensionsTests()
        {
            _urlResolverMock = new Mock<UrlResolver>();
            _httpRequestBaseMock = new Mock<HttpRequestBase>();
        }
    }
}
