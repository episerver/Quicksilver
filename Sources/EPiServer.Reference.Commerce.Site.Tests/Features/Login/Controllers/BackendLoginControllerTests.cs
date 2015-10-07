using EPiServer.Configuration;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Shared.Models.Identity;
using EPiServer.Reference.Commerce.Site.Features.Login.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Login.ViewModels;
using EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes;
using FluentAssertions;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Login.Controllers
{
    [TestClass]
    public class BackendLoginControllerTests
    {
        [TestMethod]
        public void Index_WhenReturnUrl_ShouldCreateViewModel()
        {
            const string url = "http://test.com/episerver/cms";
            var result = ((ViewResult)_subject.Index(url)).Model as BackendLoginViewModel;
            var expectedResult = new BackendLoginViewModel
            {
                ReturnUrl = url,
                Heading = "Heading",
                LoginMessage = "LoginMessage"
            };
            result.ShouldBeEquivalentTo(expectedResult);
        }

        [TestMethod]
        public void Index_WhenReturnUrlIsLocal_ShouldRedirectToReturnUrl()
        {
            var url = "http://test.com/episerver/cms";
            var result = _subject.Index(new BackendLoginViewModel
            {
                Heading = "Test Heading",
                LoginMessage = "Hello",
                Password = "stores",
                RememberMe = false,
                ReturnUrl = url,
                Username = "admin"
            });
            var redirectResult = result.Result as RedirectResult;
            Assert.AreEqual<string>(url, redirectResult.Url);
        }

        [TestMethod]
        public void Index__WhenReturnUrlIsNotLocal_ShouldRedirectToHome()
        {
            var url = "http://tester.com/episerver";
            var result = _subject.Index(new BackendLoginViewModel
            {
                Heading = "Test Heading",
                LoginMessage = "Hello",
                Password = "stores",
                RememberMe = false,
                ReturnUrl = url,
                Username = "admin"
            });
            var redirectResult = result.Result as RedirectToRouteResult;
            Assert.AreEqual<string>("Index", (string) redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void Index__WhenModelStateIsInvalid_ShouldReturnViewModel()
        {
            var url = "http://tester.com/eepiserver";
            _subject.ModelState.AddModelError("testError", "test");
            var result = ((PartialViewResult)_subject.Index(new BackendLoginViewModel
            {
                Heading = "Test Heading",
                LoginMessage = "LoginMessage",
                Password = "stores",
                RememberMe = false,
                ReturnUrl = url,
                Username = "admin"
            }).Result).Model as BackendLoginViewModel;
            var expectedResult = new BackendLoginViewModel
            {
                Heading = "Heading",
                LoginMessage = "LoginMessage",
                Password = "stores",
                RememberMe = false,
                ReturnUrl = url,
                Username = "admin"
            };
            result.ShouldBeEquivalentTo(expectedResult);
        }

        private BackendLoginController _subject;
        private CultureInfo _cultureInfo;

        [TestInitialize]
        public void Setup()
        {
            _cultureInfo = CultureInfo.CurrentUICulture;
            var english = CultureInfo.CreateSpecificCulture("en");
            Thread.CurrentThread.CurrentUICulture = english;
            var localizationService = new MemoryLocalizationService
            {
                FallbackBehavior = FallbackBehaviors.MissingMessage
            };
            localizationService.AddString(english, "/Login/BackendLogin/Heading", "Heading");
            localizationService.AddString(english, "/Login/BackendLogin/LoginMessage", "LoginMessage");
            localizationService.AddString(english, "/Login/Form/Error/WrongPasswordOrEmail", "WrongPasswordOrEmail");

            var authenticationManager = new Mock<IAuthenticationManager>();
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<ApplicationUserManager>(userStore.Object);
            var signInManager = new Mock<ApplicationSignInManager>(userManager.Object, authenticationManager.Object);
            _subject = new BackendLoginController(localizationService, signInManager.Object, new FakeUrlAuthorization());

            var request = new Mock<HttpRequestBase>();
            request.Setup(x => x.Url).Returns(new Uri("http://test.com"));
            var httpContext = new Mock<HttpContextBase>();
            httpContext.SetupGet(x => x.Request).Returns(request.Object);
            _subject.ControllerContext = new ControllerContext(httpContext.Object, new RouteData(), _subject);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Thread.CurrentThread.CurrentUICulture = _cultureInfo;
        }
    }
}
