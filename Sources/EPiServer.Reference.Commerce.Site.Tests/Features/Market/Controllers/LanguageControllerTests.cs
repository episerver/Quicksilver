using System.Linq;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Market.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Market.Models;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Web.Routing;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Market.Controllers
{
    public class LanguageControllerTests
    {
        [Fact]
        public void Index_WhenCreatingViewModel_ShouldSetLanguagesAsAvailableLanguages()
        { 
            // Arrange
            var availableLanguages = new[] { CultureInfo.GetCultureInfo("en"), CultureInfo.GetCultureInfo("sv") };
            var items = availableLanguages.Select(x => new SelectListItem
            {
                Selected = false,
                Text = x.DisplayName,
                Value = x.Name
            });

            _mockLanguageService
                .Setup(l => l.GetAvailableLanguages())
                .Returns(availableLanguages);

            // Act
            var result = ((ViewResultBase)_subject.Index(null, null)).Model as LanguageViewModel;

            // Assert
           result.Languages.ShouldBeEquivalentTo(items);
        }

        [Fact]
        public void Index_WhenLanguageIsNull_ShouldGetCurrentLanguage()
        {
            // Arrange
            string languageCode = null;

            // Act
            var result = (ViewResultBase)_subject.Index(null, languageCode);

            // Assert
            Assert.Equal<string>(_currentLanguage.Name, ((LanguageViewModel)result.Model).Language);
        }

        [Fact]
        public void Index_WhenLanguageHasValue_ShouldConvertItToCultureInfo()
        {
            // Arrange
            var languageCode = "sv";

            // Act
            var result = (ViewResultBase)_subject.Index(null, languageCode);

            // Assert
            Assert.Equal<string>(languageCode, ((LanguageViewModel)result.Model).Language);
        }

        [Fact]
        public void Set_WhenSetCurrentLanguageIsSuccessful_ShouldReturnJsonObject()
        {
            // Act
            var result = _subject.Set("en", new ContentReference(11));

            // Assert
            Assert.IsType(typeof(JsonResult), result);
        }

        [Fact]
        public void Set_WhenSetCurrentLanguageFails_ShouldReturnHttpStatusUnsupported()
        {
            // Arrange
            _mockLanguageService.Setup(x => x.SetCurrentLanguage(It.IsAny<string>())).Returns(false);

            // Act
            var result = _subject.Set("en", new ContentReference(11));

            // Assert
            Assert.Equal<int>(400, ((HttpStatusCodeResult)result).StatusCode);
        }

        [Fact]
        public void Set_WhenReturningJson_ShouldContainResolvedUrl()
        {
            // Arrange
            const string expectedUrl = "https://github.com/episerver/Quicksilver";
            var contentLink = new ContentReference(11);
            const string language = "en";

            _mockUrlResolver.Setup(x => x.GetUrl(contentLink, language)).Returns(expectedUrl);

            // Act
            var result = _subject.Set(language, contentLink);

            // Assert
            Assert.True(((JsonResult)result).Data.ToString().Contains(expectedUrl));
        }

        [Fact]
        public void Set_WhenPassingLanguage_ShouldCallSetCurrentLanguageWithLanguage()
        {
            // Arrange
            const string language = "en";

            // Act
            _subject.Set(language, new ContentReference(11));

            // Assert
            _mockLanguageService.Verify(x => x.SetCurrentLanguage(language), Times.Once);
        }

        private Mock<UrlResolver> _mockUrlResolver;
        private Mock<LanguageService> _mockLanguageService;
        private LanguageController _subject;
        private CultureInfo _currentLanguage = CultureInfo.GetCultureInfo("en");


        public LanguageControllerTests()
        {
            _mockUrlResolver = new Mock<UrlResolver>();

            _mockLanguageService = new Mock<LanguageService>(null, null, null, null);
            _mockLanguageService.Setup(x => x.SetCurrentLanguage(It.IsAny<string>())).Returns(true);
            _mockLanguageService.Setup(l => l.GetCurrentLanguage()).Returns(_currentLanguage);

            _subject = new LanguageController(_mockLanguageService.Object, _mockUrlResolver.Object);
        }
    }
}
