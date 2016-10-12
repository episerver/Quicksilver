using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using Mediachase.Commerce;
using Moq;
using System;
using System.Globalization;
using System.Web;
using System.Web.Routing;
using Xunit;


namespace EPiServer.Reference.Commerce.Site.Tests.Features.Market
{
    public class LanguageServiceTests
    {
        [Fact]
        public void UpdateReplacementLanguage_ShouldCallUpdateReplacementLanguageOnUnderlyingUpdateCurrentLanguage()
        {
            var mockContent = new Mock<IContent>();

            var service = CreateService();
            service.UpdateReplacementLanguage(mockContent.Object, English);

            _updateCurrentLanguageMock.Verify(x => x.UpdateReplacementLanguage(mockContent.Object, English), Times.Once);
        }

        [Fact]
        public void UpdateLanguage_WhenHttpContextIsNull_ShouldCallUpdateLanguageOnUnderlyingUpdateCurrentLanguageWithLanguage()
        {
            _requestContextMock.Setup(x => x.HttpContext).Returns((HttpContextBase)null);

            var service = CreateService();
            service.UpdateLanguage(English);

            _updateCurrentLanguageMock.Verify(x => x.UpdateLanguage(English), Times.Once);
        }

        [Fact]
        public void UpdateLanguage_WhenUrlIsNull_ShouldCallUpdateLanguageOnUnderlyingUpdateCurrentLanguageWithLanguage()
        {
            var mockHttpRequestBase = new Mock<HttpRequestBase>();
            mockHttpRequestBase.Setup(x => x.Url).Returns((Uri) null);

            var mockHttpContextBase = new Mock<HttpContextBase>();
            mockHttpContextBase.Setup(x => x.Request).Returns(mockHttpRequestBase.Object);

            _requestContextMock.Setup(x => x.HttpContext).Returns(mockHttpContextBase.Object);

            var service = CreateService();
            service.UpdateLanguage(English);

            _updateCurrentLanguageMock.Verify(x => x.UpdateLanguage(English), Times.Once);
        }

        [Fact]
        public void UpdateLanguage_WhenUriIsNotSiteRoot_ShouldCallUpdateLanguageOnUnderlyingUpdateCurrentLanguageWithLanguage()
        {
            var uri = new Uri("http://www.episerver.com/something");

            var mockHttpRequestBase = new Mock<HttpRequestBase>();
            mockHttpRequestBase.Setup(x => x.Url).Returns(uri);

            var mockHttpContextBase = new Mock<HttpContextBase>();
            mockHttpContextBase.Setup(x => x.Request).Returns(mockHttpRequestBase.Object);

            _requestContextMock.Setup(x => x.HttpContext).Returns(mockHttpContextBase.Object);

            var service = CreateService();
            service.UpdateLanguage(English);

            _updateCurrentLanguageMock.Verify(x => x.UpdateLanguage(English), Times.Once);
        }

        [Fact]
        public void UpdateLanguage_WhenUriIsSiteRoot_ShouldCallGetOnCookieService()
        {
            var uri = new Uri("http://www.episerver.com");

            var mockHttpRequestBase = new Mock<HttpRequestBase>();
            mockHttpRequestBase.Setup(x => x.Url).Returns(uri);

            var mockHttpContextBase = new Mock<HttpContextBase>();
            mockHttpContextBase.Setup(x => x.Request).Returns(mockHttpRequestBase.Object);

            _requestContextMock.Setup(x => x.HttpContext).Returns(mockHttpContextBase.Object);

            var service = CreateService();
            service.UpdateLanguage(null);

            _cookieServiceMock.Verify(x => x.Get("Language"), Times.Once);
        }

        [Fact]
        public void UpdateLanguage_WhenUriIsSiteRootAndCookieExist_ShouldCallUpdateLanguageOnUnderlyingUpdateCurrentLanguageWithLanguageFromCookie()
        {
            _cookieServiceMock.Setup(x => x.Get("Language")).Returns(English);

            var uri = new Uri("http://www.episerver.com");

            var mockHttpRequestBase = new Mock<HttpRequestBase>();
            mockHttpRequestBase.Setup(x => x.Url).Returns(uri);

            var mockHttpContextBase = new Mock<HttpContextBase>();
            mockHttpContextBase.Setup(x => x.Request).Returns(mockHttpRequestBase.Object);

            _requestContextMock.Setup(x => x.HttpContext).Returns(mockHttpContextBase.Object);

            var service = CreateService();
            service.UpdateLanguage(null);

            _updateCurrentLanguageMock.Verify(x => x.UpdateLanguage(English), Times.Once);
        }

        [Fact]
        public void UpdateLanguage_WhenUriIsSiteRootAndCookieExist_ShouldNotCallGetCurrentMarketOnCurrentMarket()
        {
            var uri = new Uri("http://www.episerver.com");
            
            _cookieServiceMock.Setup(x => x.Get("Language")).Returns(English);

            var mockHttpRequestBase = new Mock<HttpRequestBase>();
            mockHttpRequestBase.Setup(x => x.Url).Returns(uri);

            var mockHttpContextBase = new Mock<HttpContextBase>();
            mockHttpContextBase.Setup(x => x.Request).Returns(mockHttpRequestBase.Object);

            _requestContextMock.Setup(x => x.HttpContext).Returns(mockHttpContextBase.Object);

            var service = CreateService();
            service.UpdateLanguage(null);

            _currentMarketMock.Verify(x => x.GetCurrentMarket(), Times.Never);
        }

        [Fact]
        public void UpdateLanguage_WhenUriIsSiteRootAndCookieDontExist_ShouldCallGetCurrentMarketOnCurrentMarket()
        {
            var uri = new Uri("http://www.episerver.com");

            var mockHttpRequestBase = new Mock<HttpRequestBase>();
            mockHttpRequestBase.Setup(x => x.Url).Returns(uri);

            var mockHttpContextBase = new Mock<HttpContextBase>();
            mockHttpContextBase.Setup(x => x.Request).Returns(mockHttpRequestBase.Object);

            _requestContextMock.Setup(x => x.HttpContext).Returns(mockHttpContextBase.Object);

            var service = CreateService();
            service.UpdateLanguage(null);

            _currentMarketMock.Verify(x => x.GetCurrentMarket(), Times.Once);
        }

        [Fact]
        public void UpdateLanguage_WhenUriIsSiteRootAndCookieDontExist_ShouldCallUpdateLanguageOnUnderlyingUpdateCurrentLanguageWithCurrentMarketDefaultLanguage()
        {
            _marketMock.Setup(x => x.DefaultLanguage).Returns(CultureInfo.GetCultureInfo(English));

            var uri = new Uri("http://www.episerver.com");

            var mockHttpRequestBase = new Mock<HttpRequestBase>();
            mockHttpRequestBase.Setup(x => x.Url).Returns(uri);

            var mockHttpContextBase = new Mock<HttpContextBase>();
            mockHttpContextBase.Setup(x => x.Request).Returns(mockHttpRequestBase.Object);

            _requestContextMock.Setup(x => x.HttpContext).Returns(mockHttpContextBase.Object);

            var service = CreateService();
            service.UpdateLanguage(null);

            _updateCurrentLanguageMock.Verify(x => x.UpdateLanguage(English), Times.Once);
        }

        [Fact]
        public void SetCurrentLanguage_WhenLanguageExistsOnMarket_ShouldCallUpdateLanguageOnUnderlyingUpdateCurrentLanguage()
        {
            _marketMock.Setup(x => x.Languages).Returns(new[] { CultureInfo.GetCultureInfo(English) });

            var service = CreateService();
            service.SetCurrentLanguage(English);

            _updateCurrentLanguageMock.Verify(x => x.UpdateLanguage(English), Times.Once);
        }

        [Fact]
        public void SetCurrentLanguage_WhenLanguageDoesntExistsOnMarket_ShouldNotCallUpdateLanguageOnUnderlyingUpdateCurrentLanguage()
        {
            _marketMock.Setup(x => x.Languages).Returns(new CultureInfo[] { });

            var service = CreateService();
            service.SetCurrentLanguage(English);

            _updateCurrentLanguageMock.Verify(x => x.UpdateLanguage(English), Times.Never);
        }

        [Fact]
        public void SetCurrentLanguage_WhenLanguageExistsOnMarket_ShouldCallSetOnCookieServiceWithLanguage()
        {
            _marketMock.Setup(x => x.Languages).Returns(new[] { CultureInfo.GetCultureInfo(English) });

            var service = CreateService();
            service.SetCurrentLanguage(English);

            _cookieServiceMock.Verify(x => x.Set("Language", English), Times.Once);
        }

        [Fact]
        public void SetCurrentLanguage_WhenLanguageDoesntExistsOnMarket_ShouldNotCallSetOnCookieServiceWithLanguage()
        {
            _marketMock.Setup(x => x.Languages).Returns(new CultureInfo[] { });

            var service = CreateService();
            service.SetCurrentLanguage(English);

            _cookieServiceMock.Verify(x => x.Set("Language", English), Times.Never);
        }

        [Fact]
        public void SetCurrentLanguage_WhenLanguageExistsOnMarket_ShouldReturnTrue()
        {
            _marketMock.Setup(x => x.Languages).Returns(new[] { CultureInfo.GetCultureInfo(English) });

            var service = CreateService();
            var result = service.SetCurrentLanguage(English);

            Assert.True(result);
        }

        [Fact]
        public void SetCurrentLanguage_WhenLanguageDoesntExistsOnMarket_ShouldReturnFalse()
        {
            _marketMock.Setup(x => x.Languages).Returns(new CultureInfo[] { });

            var service = CreateService();
            var result = service.SetCurrentLanguage(English);
        
            Assert.False(result);
        }

        private const string English = "en";
        private Mock<ICurrentMarket> _currentMarketMock;
        private Mock<IMarket> _marketMock;
        private Mock<CookieService> _cookieServiceMock;
        private Mock<IUpdateCurrentLanguage> _updateCurrentLanguageMock;
        private Mock<RequestContext> _requestContextMock;
        
        public LanguageServiceTests()
        {
            _cookieServiceMock = new Mock<CookieService>();
            _updateCurrentLanguageMock = new Mock<IUpdateCurrentLanguage>();
            _marketMock = new Mock<IMarket>();

            _requestContextMock = new Mock<RequestContext>();

            _currentMarketMock = new Mock<ICurrentMarket>();
            _currentMarketMock.Setup(x => x.SetCurrentMarket(It.IsAny<string>())).Callback(() => { });
            _currentMarketMock.Setup(x => x.GetCurrentMarket()).Returns(_marketMock.Object);
        }

        private LanguageService CreateService()
        {
            return new LanguageService(_currentMarketMock.Object, _cookieServiceMock.Object, _updateCurrentLanguageMock.Object, _requestContextMock.Object);
        }
    }
}
