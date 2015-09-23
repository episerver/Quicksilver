using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Shared.Models.Identity;
using EPiServer.Reference.Commerce.Site.Features.Login.Services;
using EPiServer.Reference.Commerce.Site.Features.ResetPassword.Controllers;
using EPiServer.Reference.Commerce.Site.Features.ResetPassword.Pages;
using EPiServer.Reference.Commerce.Site.Features.ResetPassword.ViewModels;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.ResetPassword.Controllers
{
    [TestClass]
    public class ResetPasswordMailControllerTests
    {
        [TestMethod]
        public void Index_WhenIdIsNotEmpty_ShouldReturnCallbackUrl()
        {
            string userId = Guid.NewGuid().ToString();
            ViewResult result = _subject.Index(_resetPasswordMailPageMock.Object, userId, "en").Result as ViewResult;
            ResetPasswordMailModel viewModel = result.Model as ResetPasswordMailModel;
            Assert.IsNotNull(viewModel.CallbackUrl);
        }

        [TestMethod]
        public void Index_WhenIdIsEmpty_ShouldReturnCallbackUrlAsNull()
        {
            ViewResult result = _subject.Index(_resetPasswordMailPageMock.Object, null, "en").Result as ViewResult;
            ResetPasswordMailModel viewModel = result.Model as ResetPasswordMailModel;
            Assert.IsNull(viewModel.CallbackUrl);
        }

        Mock<ResetPasswordMailPage> _resetPasswordMailPageMock;
        ResetPasswordMailController _subject;

        [TestInitialize]
        public void Setup()
        {
            MemoryLocalizationService localizationService = new MemoryLocalizationService();
            Mock<IUserStore<ApplicationUser>> userStore = new Mock<IUserStore<ApplicationUser>>();
            Mock<IAuthenticationManager> authenticationManager = new Mock<IAuthenticationManager>();
            Mock<IContentLoader> contentLoaderMock = new Mock<IContentLoader>();
            Mock<CustomerContextFacade> customerContexttFacade = new Mock<CustomerContextFacade>();
            Mock<ApplicationUserManager> userManagerMock = new Mock<ApplicationUserManager>(userStore.Object);
            Mock<ApplicationSignInManager> signinManagerMock = new Mock<ApplicationSignInManager>(userManagerMock.Object, authenticationManager.Object);
            Mock<UserService> userServiceMock = new Mock<UserService>(userManagerMock.Object, signinManagerMock.Object, authenticationManager.Object, localizationService, customerContexttFacade.Object);
            Mock<HttpRequestBase> requestMock = new Mock<HttpRequestBase>();
            Mock<HttpContextBase> httpContextMock = new Mock<HttpContextBase>();
            Mock<UrlHelper> urlHelper = new Mock<UrlHelper>();
            _resetPasswordMailPageMock = new Mock<ResetPasswordMailPage>();

            requestMock.Setup(x => x.Url).Returns(new Uri("https://www.quicksilver.com"));
            urlHelper.Setup(x => x.Action(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>())).Returns("http://www.quicksilver.com/resetpassword/resetpassword");
            httpContextMock.SetupGet(x => x.Request).Returns(requestMock.Object);

            _subject = new ResetPasswordMailController(signinManagerMock.Object, userManagerMock.Object, userServiceMock.Object);
            _subject.ControllerContext = new ControllerContext(httpContextMock.Object, new RouteData(), _subject);
            _subject.Url = urlHelper.Object;
        }
    }
}
