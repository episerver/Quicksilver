using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using Mediachase.Commerce;
using Moq;
using System.Globalization;
using Xunit;


namespace EPiServer.Reference.Commerce.Site.Tests.Features.Market
{
    public class LanguageServiceTests
    {
        [Fact]
        public void UpdateReplacementLanguage_ShouldCallUpdateReplacementLanguageOnUnderlyingUpdateCurrentLanguage()
        {
            var mockContent = new Mock<IContent>();

            _subject.UpdateReplacementLanguage(mockContent.Object, English);

            _updateCurrentLanguageMock.Verify(x => x.UpdateReplacementLanguage(mockContent.Object, English), Times.Once);
        }

        [Fact]
        public void UpdateLanguage_WhenLanguageIdIsNotNull_ShouldCallUpdateLanguageOnUnderlyingUpdateCurrentLanguageWithLanguage()
        {
            _subject.UpdateLanguage(English);

            _updateCurrentLanguageMock.Verify(x => x.UpdateLanguage(English), Times.Once);
        }

        [Fact]
        public void UpdateLanguage_WhenLanguageIdIsNull_ShouldCallGetOnCookieService()
        {
            _subject.UpdateLanguage(null);

            _cookieServiceMock.Verify(x => x.Get("Language"), Times.Once);
        }

        [Fact]
        public void UpdateLanguage_WhenLanguageIdIsNullAndCookieExists_ShouldCallUpdateLanguageOnUnderlyingUpdateCurrentLanguageWithLanguageFromCookie()
        {
            _cookieServiceMock.Setup(x => x.Get("Language")).Returns(English);

            _subject.UpdateLanguage(null);

            _updateCurrentLanguageMock.Verify(x => x.UpdateLanguage(English), Times.Once);
        }

        [Fact]
        public void UpdateLanguage_WhenLanguageIdIsNullAndCookieExists_ShouldNotCallGetCurrentMarketOnCurrentMarket()
        {
            _cookieServiceMock.Setup(x => x.Get("Language")).Returns(English);

            _subject.UpdateLanguage(null);

            _currentMarketMock.Verify(x => x.GetCurrentMarket(), Times.Never);
        }

        [Fact]
        public void UpdateLanguage_WhenLanguageIdIsNullAndCookieDoesNotExist_ShouldCallGetCurrentMarketOnCurrentMarket()
        {
            _subject.UpdateLanguage(null);

            _currentMarketMock.Verify(x => x.GetCurrentMarket(), Times.Once);
        }

        [Fact]
        public void UpdateLanguage_WhenLanguageIdIsNullAndCookieDoesNotExist_ShouldCallUpdateLanguageOnUnderlyingUpdateCurrentLanguageWithCurrentMarketDefaultLanguage()
        {
            _marketMock.Setup(x => x.DefaultLanguage).Returns(CultureInfo.GetCultureInfo(English));

            _subject.UpdateLanguage(null);

            _updateCurrentLanguageMock.Verify(x => x.UpdateLanguage(English), Times.Once);
        }

        [Fact]
        public void UpdateLanguage_WhenLanguageExistsOnMarket_ShouldCallUpdateLanguageOnUnderlyingUpdateCurrentLanguage()
        {
            _marketMock.Setup(x => x.Languages).Returns(new[] { CultureInfo.GetCultureInfo(English) });

            _subject.UpdateLanguage(English);

            _updateCurrentLanguageMock.Verify(x => x.UpdateLanguage(English), Times.Once);
        }

        [Fact]
        public void UpdateLanguage_WhenLanguageIdIsNotNullAndIsDifferentThanCookieLanguage_ShouldCallSetOnCookieServiceWithLanguage()
        {
            _cookieServiceMock.Setup(x => x.Get("Language")).Returns("sv");

            _subject.UpdateLanguage(English);

            _cookieServiceMock.Verify(x => x.Set("Language", English), Times.Once);
        }

        [Fact]
        public void UpdateLanguage_WhenLanguageIdIsNotNullAndIsSameAsCookieLanguage_ShouldNotCallSetOnCookieService()
        {
            _cookieServiceMock.Setup(x => x.Get("Language")).Returns(English);

            _subject.UpdateLanguage(English);

            _cookieServiceMock.Verify(x => x.Set("Language", English), Times.Never);
        }

        private LanguageService _subject;
        private const string English = "en";
        private Mock<ICurrentMarket> _currentMarketMock;
        private Mock<IMarket> _marketMock;
        private Mock<CookieService> _cookieServiceMock;
        private Mock<IUpdateCurrentLanguage> _updateCurrentLanguageMock;
        
        public LanguageServiceTests()
        {
            _cookieServiceMock = new Mock<CookieService>();
            _updateCurrentLanguageMock = new Mock<IUpdateCurrentLanguage>();
            _marketMock = new Mock<IMarket>();
            _currentMarketMock = new Mock<ICurrentMarket>();
            _currentMarketMock.Setup(x => x.GetCurrentMarket()).Returns(_marketMock.Object);

            _subject = new LanguageService(_currentMarketMock.Object, _cookieServiceMock.Object, _updateCurrentLanguageMock.Object);
        }
    }
}
