using EPiServer.Reference.Commerce.Shared.Models.Identity;
using EPiServer.Reference.Commerce.Site.Features.Login.Pages;
using EPiServer.Reference.Commerce.Site.Features.Login.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Controllers;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Shared.Controllers
{
    [TestClass]
    public class IdentityControllerBaseTests
    {
        [TestMethod]
        public void RedirectToLocal_WhenReturnUrlIsLocal_ShouldRedirectToReturnUrl()
        {
            const string url = "http://test.com/episerver";
            var result = _subject.RedirectToLocal(url);
            var redirectResult = result as RedirectResult;
            Assert.AreEqual(url, redirectResult.Url);
        }

        [TestMethod]
        public void RedirectToLocal_WhenReturnUrlIsNotLocal_ShouldRedirectToHome()
        {
            const string url = "http://tester.com/episerver";
            var result = _subject.RedirectToLocal(url);
            var redirectResult = result as RedirectToRouteResult;
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void SignOut_ShouldRedirectToHome()
        {
            var result = _subject.SignOut();
            var redirectResult = result as RedirectToRouteResult;
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
        }

        private FakeLoginController _subject;
        
        [TestInitialize]
        public void Setup()
        {
            var authenticationManager = new Mock<IAuthenticationManager>();
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<ApplicationUserManager>(userStore.Object);
            var customercontextFacadeMock = new Mock<CustomerContextFacade>();
            var signInManager = new Mock<ApplicationSignInManager>(userManager.Object, authenticationManager.Object);
            var request = new Mock<HttpRequestBase>();
            var httpContext = new Mock<HttpContextBase>();
            var userService = new UserService(userManager.Object, signInManager.Object, authenticationManager.Object, null, customercontextFacadeMock.Object);
            
            request.Setup(
                x => x.Url)
                .Returns(new Uri("http://test.com"));
            
            httpContext.SetupGet(
                x => x.Request)
                .Returns(request.Object);

            _subject = new FakeLoginController(signInManager.Object, userManager.Object, userService);
            _subject.ControllerContext = new ControllerContext(httpContext.Object, new RouteData(), _subject);
        }

        private class FakeLoginController : IdentityControllerBase<LoginRegistrationPage>
        {
            public FakeLoginController(ApplicationSignInManager applicationSignInManager, ApplicationUserManager applicationUserManager, UserService userService)
                : base(applicationSignInManager, applicationUserManager, userService)
            {

            }
        }
    }
}
