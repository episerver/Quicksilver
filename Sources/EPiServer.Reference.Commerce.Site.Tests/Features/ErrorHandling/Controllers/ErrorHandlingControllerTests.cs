using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.ErrorHandling.Controllers;
using EPiServer.Reference.Commerce.Site.Features.ErrorHandling.Models;
using EPiServer.Reference.Commerce.Site.Features.ErrorHandling.Pages;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Web.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.ErrorHandling.Controllers
{
    [TestClass]
    public class ErrorHandlingControllerTests
    {
        [TestMethod]
        public void Index_WhenPassingPage_ShouldUseItOnModel()
        {
            // Arrange
            var errorPage = new ErrorPage();

            // Act
            var result = (ViewResult)_subject.Index(errorPage);

            // Assert
            Assert.AreEqual<ErrorPage>(errorPage, ((ErrorViewModel)result.Model).CurrentPage);
        }

        [TestMethod]
        public void PageNotFound_WhenUrlCanBeResolved_ShouldRedirectToUrl()
        {
            // Arrange
            var resolvedUrl = "http://example.com/Resolved/";
            _urlResolver.Setup(u => u.GetUrl(It.IsAny<ContentReference>())).Returns(resolvedUrl);

            // Act
            var result = (RedirectResult)_subject.PageNotFound();

            // Assert
            Assert.AreEqual<string>(resolvedUrl, result.Url);
        }

        [TestMethod]
        public void PageNotFound_WhenUrlResolvingThrows_ShouldRedirectToFallback()
        {
            // Arrange
            _urlResolver.Setup(u => u.GetUrl(It.IsAny<ContentReference>())).Throws(new EPiServerException("Oops"));

            // Act
            var result = (RedirectResult)_subject.PageNotFound();

            // Assert
            Assert.AreEqual<string>(_fallbackUrl, result.Url);
        }

        [TestMethod]
        public void PageNotFound_WhenUrlResolvingReturnsNull_ShouldRedirectToFallback()
        {
            // Arrange
            _urlResolver.Setup(u => u.GetUrl(It.IsAny<ContentReference>())).Returns((String)null);

            // Act
            var result = (RedirectResult)_subject.PageNotFound();

            // Assert
            Assert.AreEqual<string>(_fallbackUrl, result.Url);
        }

        ErrorHandlingController _subject;
        Mock<IContentLoader> _contentLoaderMock;
        Mock<UrlResolver> _urlResolver;
        string _fallbackUrl = "~/Features/ErrorHandling/Pages/ErrorFallback.html";

        [TestInitialize]
        public void Setup()
        {
            _contentLoaderMock = new Mock<IContentLoader>();
            _contentLoaderMock.Setup(c => c.Get<StartPage>(It.IsAny<ContentReference>())).Returns(new StartPage());
            _urlResolver = new Mock<UrlResolver>();

            _subject = new ErrorHandlingController(_contentLoaderMock.Object, _urlResolver.Object);
        }
    }
}
