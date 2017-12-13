using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Reference.Commerce.Shared.Identity;
using EPiServer.Reference.Commerce.Site.Features.Login.Pages;
using EPiServer.Reference.Commerce.Site.Features.Login.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Controllers;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Moq;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Xunit;


namespace EPiServer.Reference.Commerce.Site.Tests.Features.Shared.Controllers
{
    public class IdentityControllerBaseTests
    {
        [Fact]
        public void RedirectToLocal_WhenReturnUrlIsLocal_ShouldRedirectToReturnUrl()
        {
            const string url = "http://test.com/episerver";
            var result = _subject.RedirectToLocal(url);
            var redirectResult = result as RedirectResult;
            Assert.Equal(url, redirectResult.Url);
        }

        [Fact]
        public void RedirectToLocal_WhenReturnUrlIsNotLocal_ShouldRedirectToHome()
        {
            const string url = "http://tester.com/episerver";
            var result = _subject.RedirectToLocal(url);
            var redirectResult = result as RedirectToRouteResult;
            Assert.Equal("Index", redirectResult.RouteValues["action"]);
        }

        [Fact]
        public void SignOut_ShouldRedirectToHome()
        {
            var result = _subject.SignOut();
            var redirectResult = result as RedirectToRouteResult;
            Assert.Equal("Index", redirectResult.RouteValues["action"]);
        }

        private FakeLoginController _subject;


        public IdentityControllerBaseTests()
        {
            var authenticationManager = new Mock<IAuthenticationManager>();
            var userStore = new Mock<IUserStore<SiteUser>>();
            var userManager = new Mock<ApplicationUserManager<SiteUser>>(userStore.Object);
            var customercontextFacadeMock = new Mock<CustomerContextFacade>(null);
            var signInManager = new Mock<ApplicationSignInManager<SiteUser>>(userManager.Object, authenticationManager.Object, new ApplicationOptions());
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
            public FakeLoginController(ApplicationSignInManager<SiteUser> applicationSignInManager, ApplicationUserManager<SiteUser> userManager, UserService userService)
                : base(applicationSignInManager, userManager, userService)
            {

            }
        }
    }
}
