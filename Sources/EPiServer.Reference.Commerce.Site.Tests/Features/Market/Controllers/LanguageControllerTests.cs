using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.Reference.Commerce.Site.Features.Market.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Market.ViewModels;
using EPiServer.Web.Routing;
using FluentAssertions;
using Moq;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Market.Controllers
{
    public class LanguageControllerTests
    {
        [Fact]
        public void Index_WhenCreatingViewModel_ShouldSetLanguagesAsAvailableLanguages()
        { 
            var availableLanguages = new[] { CultureInfo.GetCultureInfo("en"), CultureInfo.GetCultureInfo("sv") };
            var items = availableLanguages.Select(x => new SelectListItem
            {
                Selected = false,
                Text = x.DisplayName,
                Value = x.Name
            });

            _languageServiceMock
                .Setup(l => l.GetAvailableLanguages())
                .Returns(availableLanguages);

            var result = ((ViewResultBase)_subject.Index(null, null)).Model as LanguageViewModel;

           result.Languages.Should().BeEquivalentTo(items);
        }

        [Fact]
        public void Index_WhenLanguageIsNull_ShouldGetCurrentLanguage()
        {
            string languageCode = null;

            var result = (ViewResultBase)_subject.Index(null, languageCode);

            Assert.Equal(_currentLanguage.Name, ((LanguageViewModel)result.Model).Language);
        }

        [Fact]
        public void Index_WhenLanguageHasValue_ShouldConvertItToCultureInfo()
        {
            var languageCode = "sv";

            var result = (ViewResultBase)_subject.Index(null, languageCode);

            Assert.Equal(languageCode, ((LanguageViewModel)result.Model).Language);
        }

        [Fact]
        public void Set_WhenSetCurrentLanguageIsSuccessful_ShouldReturnJsonObject()
        {
            var result = _subject.Set("en", new ContentReference(11));

            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public void Set_WhenReturningJson_ShouldContainResolvedUrl()
        {
            const string expectedUrl = "https://github.com/episerver/Quicksilver";
            var contentLink = new ContentReference(11);
            var languageCode = "en";

            _urlResolverMock.Setup(x => x.GetUrl(contentLink, languageCode)).Returns(expectedUrl);

            var result = _subject.Set(languageCode, contentLink);

            Assert.Contains(expectedUrl, ((JsonResult)result).Data.ToString());
        }

        private Mock<UrlResolver> _urlResolverMock;
        private Mock<IUpdateCurrentLanguage> _updateCurrentLanguageMock;
        private Mock<CookieService> _cookieServiceMock;
        private Mock<LanguageService> _languageServiceMock;
        private LanguageController _subject;
        private CultureInfo _currentLanguage = CultureInfo.GetCultureInfo("en");

        public LanguageControllerTests()
        {
            _urlResolverMock = new Mock<UrlResolver>();
            _updateCurrentLanguageMock = new Mock<IUpdateCurrentLanguage>();
            _cookieServiceMock = new Mock<CookieService>();
            _languageServiceMock = new Mock<LanguageService>(null, _cookieServiceMock.Object, _updateCurrentLanguageMock.Object);
            _languageServiceMock.Setup(l => l.GetCurrentLanguage()).Returns(_currentLanguage);

            _subject = new LanguageController(_languageServiceMock.Object, _urlResolverMock.Object);
        }
    }
}