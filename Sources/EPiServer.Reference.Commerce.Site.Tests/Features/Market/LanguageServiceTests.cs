using System;
using System.Globalization;
using System.Web;
using System.Web.Routing;
using EPiServer.Core;
using EPiServer.Globalization;
using Mediachase.Commerce;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Market
{
    [TestClass]
    public class LanguageServiceTests
    {
        [TestMethod]
        public void UpdateReplacementLanguage_ShouldCallUpdateReplacementLanguageOnUnderlyingUpdateCurrentLanguage()
        {
            var mockContent = new Mock<IContent>();

            var service = CreateService();
            service.UpdateReplacementLanguage(mockContent.Object, English);

            _mockUpdateCurrentLanguage.Verify(x => x.UpdateReplacementLanguage(mockContent.Object, English), Times.Once);
        }

        [TestMethod]
        public void UpdateLanguage_WhenHttpContextIsNull_ShouldCallUpdateLanguageOnUnderlyingUpdateCurrentLanguageWithLanguage()
        {
            _requestContext.Setup(x => x.HttpContext).Returns((HttpContextBase)null);

            var service = CreateService();
            service.UpdateLanguage(English);

            _mockUpdateCurrentLanguage.Verify(x => x.UpdateLanguage(English), Times.Once);
        }

        [TestMethod]
        public void UpdateLanguage_WhenUrlIsNull_ShouldCallUpdateLanguageOnUnderlyingUpdateCurrentLanguageWithLanguage()
        {
            var mockHttpRequestBase = new Mock<HttpRequestBase>();
            mockHttpRequestBase.Setup(x => x.Url).Returns((Uri) null);

            var mockHttpContextBase = new Mock<HttpContextBase>();
            mockHttpContextBase.Setup(x => x.Request).Returns(mockHttpRequestBase.Object);

            _requestContext.Setup(x => x.HttpContext).Returns(mockHttpContextBase.Object);

            var service = CreateService();
            service.UpdateLanguage(English);

            _mockUpdateCurrentLanguage.Verify(x => x.UpdateLanguage(English), Times.Once);
        }

        [TestMethod]
        public void UpdateLanguage_WhenUriIsNotSiteRoot_ShouldCallUpdateLanguageOnUnderlyingUpdateCurrentLanguageWithLanguage()
        {
            var uri = new Uri("http://www.episerver.com/something");

            var mockHttpRequestBase = new Mock<HttpRequestBase>();
            mockHttpRequestBase.Setup(x => x.Url).Returns(uri);

            var mockHttpContextBase = new Mock<HttpContextBase>();
            mockHttpContextBase.Setup(x => x.Request).Returns(mockHttpRequestBase.Object);

            _requestContext.Setup(x => x.HttpContext).Returns(mockHttpContextBase.Object);

            var service = CreateService();
            service.UpdateLanguage(English);

            _mockUpdateCurrentLanguage.Verify(x => x.UpdateLanguage(English), Times.Once);
        }

        [TestMethod]
        public void UpdateLanguage_WhenUriIsSiteRoot_ShouldCallGetOnCookieService()
        {
            var uri = new Uri("http://www.episerver.com");

            var mockHttpRequestBase = new Mock<HttpRequestBase>();
            mockHttpRequestBase.Setup(x => x.Url).Returns(uri);

            var mockHttpContextBase = new Mock<HttpContextBase>();
            mockHttpContextBase.Setup(x => x.Request).Returns(mockHttpRequestBase.Object);

            _requestContext.Setup(x => x.HttpContext).Returns(mockHttpContextBase.Object);

            var service = CreateService();
            service.UpdateLanguage(null);

            _mockCookieService.Verify(x => x.Get("Language"), Times.Once);
        }

        [TestMethod]
        public void UpdateLanguage_WhenUriIsSiteRootAndCookieExist_ShouldCallUpdateLanguageOnUnderlyingUpdateCurrentLanguageWithLanguageFromCookie()
        {
            _mockCookieService.Setup(x => x.Get("Language")).Returns(English);

            var uri = new Uri("http://www.episerver.com");

            var mockHttpRequestBase = new Mock<HttpRequestBase>();
            mockHttpRequestBase.Setup(x => x.Url).Returns(uri);

            var mockHttpContextBase = new Mock<HttpContextBase>();
            mockHttpContextBase.Setup(x => x.Request).Returns(mockHttpRequestBase.Object);

            _requestContext.Setup(x => x.HttpContext).Returns(mockHttpContextBase.Object);

            var service = CreateService();
            service.UpdateLanguage(null);

            _mockUpdateCurrentLanguage.Verify(x => x.UpdateLanguage(English), Times.Once);
        }

        [TestMethod]
        public void UpdateLanguage_WhenUriIsSiteRootAndCookieExist_ShouldNotCallGetCurrentMarketOnCurrentMarket()
        {
            var uri = new Uri("http://www.episerver.com");
            
            _mockCookieService.Setup(x => x.Get("Language")).Returns(English);

            var mockHttpRequestBase = new Mock<HttpRequestBase>();
            mockHttpRequestBase.Setup(x => x.Url).Returns(uri);

            var mockHttpContextBase = new Mock<HttpContextBase>();
            mockHttpContextBase.Setup(x => x.Request).Returns(mockHttpRequestBase.Object);

            _requestContext.Setup(x => x.HttpContext).Returns(mockHttpContextBase.Object);

            var service = CreateService();
            service.UpdateLanguage(null);

            _mockCurrentMarket.Verify(x => x.GetCurrentMarket(), Times.Never);
        }

        [TestMethod]
        public void UpdateLanguage_WhenUriIsSiteRootAndCookieDontExist_ShouldCallGetCurrentMarketOnCurrentMarket()
        {
            var uri = new Uri("http://www.episerver.com");

            var mockHttpRequestBase = new Mock<HttpRequestBase>();
            mockHttpRequestBase.Setup(x => x.Url).Returns(uri);

            var mockHttpContextBase = new Mock<HttpContextBase>();
            mockHttpContextBase.Setup(x => x.Request).Returns(mockHttpRequestBase.Object);

            _requestContext.Setup(x => x.HttpContext).Returns(mockHttpContextBase.Object);

            var service = CreateService();
            service.UpdateLanguage(null);

            _mockCurrentMarket.Verify(x => x.GetCurrentMarket(), Times.Once);
        }

        [TestMethod]
        public void UpdateLanguage_WhenUriIsSiteRootAndCookieDontExist_ShouldCallUpdateLanguageOnUnderlyingUpdateCurrentLanguageWithCurrentMarketDefaultLanguage()
        {
            _mockMarket.Setup(x => x.DefaultLanguage).Returns(CultureInfo.GetCultureInfo(English));

            var uri = new Uri("http://www.episerver.com");

            var mockHttpRequestBase = new Mock<HttpRequestBase>();
            mockHttpRequestBase.Setup(x => x.Url).Returns(uri);

            var mockHttpContextBase = new Mock<HttpContextBase>();
            mockHttpContextBase.Setup(x => x.Request).Returns(mockHttpRequestBase.Object);

            _requestContext.Setup(x => x.HttpContext).Returns(mockHttpContextBase.Object);

            var service = CreateService();
            service.UpdateLanguage(null);

            _mockUpdateCurrentLanguage.Verify(x => x.UpdateLanguage(English), Times.Once);
        }

        [TestMethod]
        public void SetCurrentLanguage_WhenLanguageExistsOnMarket_ShouldCallUpdateLanguageOnUnderlyingUpdateCurrentLanguage()
        {
            _mockMarket.Setup(x => x.Languages).Returns(new[] { CultureInfo.GetCultureInfo(English) });

            var service = CreateService();
            service.SetCurrentLanguage(English);

            _mockUpdateCurrentLanguage.Verify(x => x.UpdateLanguage(English), Times.Once);
        }

        [TestMethod]
        public void SetCurrentLanguage_WhenLanguageDoesntExistsOnMarket_ShouldNotCallUpdateLanguageOnUnderlyingUpdateCurrentLanguage()
        {
            _mockMarket.Setup(x => x.Languages).Returns(new CultureInfo[] { });

            var service = CreateService();
            service.SetCurrentLanguage(English);

            _mockUpdateCurrentLanguage.Verify(x => x.UpdateLanguage(English), Times.Never);
        }

        [TestMethod]
        public void SetCurrentLanguage_WhenLanguageExistsOnMarket_ShouldCallSetOnCookieServiceWithLanguage()
        {
            _mockMarket.Setup(x => x.Languages).Returns(new[] { CultureInfo.GetCultureInfo(English) });

            var service = CreateService();
            service.SetCurrentLanguage(English);

            _mockCookieService.Verify(x => x.Set("Language", English), Times.Once);
        }

        [TestMethod]
        public void SetCurrentLanguage_WhenLanguageDoesntExistsOnMarket_ShouldNotCallSetOnCookieServiceWithLanguage()
        {
            _mockMarket.Setup(x => x.Languages).Returns(new CultureInfo[] { });

            var service = CreateService();
            service.SetCurrentLanguage(English);

            _mockCookieService.Verify(x => x.Set("Language", English), Times.Never);
        }

        [TestMethod]
        public void SetCurrentLanguage_WhenLanguageExistsOnMarket_ShouldReturnTrue()
        {
            _mockMarket.Setup(x => x.Languages).Returns(new[] { CultureInfo.GetCultureInfo(English) });

            var service = CreateService();
            var result = service.SetCurrentLanguage(English);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void SetCurrentLanguage_WhenLanguageDoesntExistsOnMarket_ShouldReturnFalse()
        {
            _mockMarket.Setup(x => x.Languages).Returns(new CultureInfo[] { });

            var service = CreateService();
            var result = service.SetCurrentLanguage(English);
        
            Assert.IsFalse(result);
        }

        private const string English = "en";

        private Mock<ICurrentMarket> _mockCurrentMarket;
        private Mock<IMarket> _mockMarket;
        private Mock<CookieService> _mockCookieService;
        private Mock<IUpdateCurrentLanguage> _mockUpdateCurrentLanguage;
        private Mock<RequestContext> _requestContext;

        [TestInitialize]
        public void Setup()
        {
            _mockCookieService = new Mock<CookieService>();
            _mockUpdateCurrentLanguage = new Mock<IUpdateCurrentLanguage>();
            _mockMarket = new Mock<IMarket>();

            _requestContext = new Mock<RequestContext>();

            _mockCurrentMarket = new Mock<ICurrentMarket>();
            _mockCurrentMarket.Setup(x => x.SetCurrentMarket(It.IsAny<string>())).Callback(() => { });
            _mockCurrentMarket.Setup(x => x.GetCurrentMarket()).Returns(_mockMarket.Object);
        }

        private LanguageService CreateService()
        {
            return new LanguageService(_mockCurrentMarket.Object, _mockCookieService.Object, _mockUpdateCurrentLanguage.Object, _requestContext.Object);
        }
    }
}
