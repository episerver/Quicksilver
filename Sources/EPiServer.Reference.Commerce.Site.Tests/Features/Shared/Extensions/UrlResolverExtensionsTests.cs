using System;
using System.Web;
using EPiServer.Core;
using EPiServer.Web.Routing;
using Moq;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Shared.Extensions
{
    public class UrlResolverExtensionsTests
    {
        [Fact]
        public void UrlResolverExtensions_WhenContentLinkIsNull_ShouldReturnPathAndQueryFromRequestUrlReferrer()
        {
            var expectedUrlReferrer = new Uri("https://github.com/episerver/Quicksilver");
            _mockHttpRequestBase.Setup(x => x.UrlReferrer).Returns(expectedUrlReferrer);

            var result = Site.Features.Shared.Extensions.UrlResolverExtensions.GetUrl(_mockUrlResolver.Object, _mockHttpRequestBase.Object, null, "en");
            Assert.Equal<string>(expectedUrlReferrer.PathAndQuery, result);
        }

        [Fact]
        public void UrlResolverExtensions_WhenContentLinkIsNullAndUrlReferrerOnRequestIsNull_ShouldReturnSlash()
        {
            _mockHttpRequestBase.Setup(x => x.UrlReferrer).Returns((Uri)null);

            var result = Site.Features.Shared.Extensions.UrlResolverExtensions.GetUrl(_mockUrlResolver.Object, _mockHttpRequestBase.Object, null, "en");
            Assert.Equal<string>("/", result);
        }

        [Fact]
        public void UrlResolverExtensions_WhenContentLinkIsEmpty_ShouldReturnPathAndQueryFromRequestUrlReferrer()
        {
            var expectedUrlReferrer = new Uri("https://github.com/episerver/Quicksilver");
            _mockHttpRequestBase.Setup(x => x.UrlReferrer).Returns(expectedUrlReferrer);

            var result = Site.Features.Shared.Extensions.UrlResolverExtensions.GetUrl(_mockUrlResolver.Object, _mockHttpRequestBase.Object, ContentReference.EmptyReference, "en");
            Assert.Equal<string>(expectedUrlReferrer.PathAndQuery, result);
        }

        [Fact]
        public void UrlResolverExtensions_WhenContentLinkIsSet_ShouldReturnResultFromGetUrlOnUrlResolver()
        {
            const string expectedUrl = "https://github.com/episerver/Quicksilver";

            var contentLink = new ContentReference(11);
            const string language = "en";

            _mockUrlResolver.Setup(x => x.GetUrl(contentLink, language)).Returns(expectedUrl);

            var result = Site.Features.Shared.Extensions.UrlResolverExtensions.GetUrl(_mockUrlResolver.Object, _mockHttpRequestBase.Object, contentLink, language);
            Assert.Equal<string>(expectedUrl, result);
        }

        private Mock<UrlResolver> _mockUrlResolver;
        private Mock<HttpRequestBase> _mockHttpRequestBase;


        public UrlResolverExtensionsTests()
        {
            _mockUrlResolver = new Mock<UrlResolver>();
            _mockHttpRequestBase = new Mock<HttpRequestBase>();
        }
    }
}
