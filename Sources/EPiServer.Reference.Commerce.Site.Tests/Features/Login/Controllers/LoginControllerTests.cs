using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Shared.Models.Identity;
using EPiServer.Reference.Commerce.Site.Features.Login.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Login.Pages;
using EPiServer.Reference.Commerce.Site.Features.Login.Services;
using EPiServer.Reference.Commerce.Site.Features.Login.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using FluentAssertions;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders.Dto;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Login.Controllers
{
    [TestClass]
    public class LoginControllerTests
    {
        [TestMethod]
        public void Index_WhenCurrentPageAndReturnUrl_ShouldCreateViewModel()
        {
            var page = new LoginRegistrationPage();
            var result = ((ViewResult)_subject.Index(page, _testUrl)).Model as LoginPageViewModel;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Index_WhenCurrentPageAndReturnUrl_ShouldPassPageToViewModel()
        {
            var page = new LoginRegistrationPage();
            var result = ((ViewResult)_subject.Index(page, _testUrl)).Model as LoginPageViewModel;

            Assert.AreEqual(page, result.CurrentPage);
        }

        [TestMethod]
        public void Index_WhenCurrentPageAndReturnUrl_ShouldPassUrlToViewModel()
        {
            var page = new LoginRegistrationPage();
            var result = ((ViewResult)_subject.Index(page, _testUrl)).Model as LoginPageViewModel;

            Assert.AreEqual(_testUrl, result.LoginViewModel.ReturnUrl);
        }

        [TestMethod]
        public void RegisterAccount_WhenRegisterSuccessful_ShouldReturnJsonReturnUrl()
        {
            var identityResult = new IdentityResult();
            typeof(IdentityResult).GetProperty("Succeeded").SetValue(identityResult, true, null);

            _userServiceMock.Setup(
                x => x.RegisterAccount(It.IsAny<ApplicationUser>()))
                .Returns(Task.FromResult(new ContactIdentityResult
                (
                    identityResult,
                    CustomerContact.CreateInstance()
                )));

            var model = new RegisterAccountViewModel
            {
                Email = "email@email.com",
                Newsletter = true,
                Password = "Passwors@124#212",
                Password2 = "Passwors@124#212",
            };

            model.Address = new Address
            {
                Line1 = "Address",
                City = "City",
                CountryName = "Country",
                FirstName = "Fisrt Name",
                LastName = "Last Name",
                PostalCode = "952595",
                Email = "email@email.com"
            };

            var result = _subject.RegisterAccount(model).Result as JsonResult;

            var expectedResult = new JsonResult
            {
                Data = new { ReturnUrl = "/" },
                JsonRequestBehavior = JsonRequestBehavior.DenyGet
            };

            result.ShouldBeEquivalentTo(expectedResult);
        }

        [TestMethod]
        public void RegisterAccount_WhenRegisterFails_ShouldReturnModelErrorState()
        {
            _userServiceMock.Setup(
                x => x.RegisterAccount(It.IsAny<ApplicationUser>()))
                .Returns(Task.FromResult(new ContactIdentityResult
                (
                    new IdentityResult("We have an error"),
                    CustomerContact.CreateInstance()
                )));

            var model = new RegisterAccountViewModel
            {
                Email = "email@email.com",
                Newsletter = true,
                Password = "Passwors@124#212",
                Password2 = "Passwors@124#212",
            };

            model.Address = new Address
            {
                Line1 = "Address",
                City = "City",
                CountryName = "Country",
                FirstName = "Fisrt Name",
                LastName = "Last Name",
                PostalCode = "952595",
                HtmlFieldPrefix = "Address"
            };

            var result = _subject.RegisterAccount(model);

            _subject.ModelState.Values.First().Errors.First().ErrorMessage.Should().Be("We have an error");
        }

        [TestMethod]
        public void OnException_ShouldDelegateToExceptionHandler()
        {
            _subject.CallOnException(_exceptionContext);
            _controllerExceptionHandler.Verify(x => x.HandleRequestValidationException(_exceptionContext, "registeraccount", _subject.OnRegisterException));
        }

        [TestMethod]
        public void OnRegisterException_ShouldCreateRegisterAccountViewModel()
        {
            //Setup
            {
                Setup_exception(new HttpRequestValidationException());
                Setup_action_for_controller("registeraccount");
            }

            var result = _subject.OnRegisterException(_exceptionContext);

            Assert.IsInstanceOfType(((ViewResult)result).Model, typeof(RegisterAccountViewModel));
        }

        [TestMethod]
        public void InternalLogin_WhenSuccessful_ShouldReturnJson()
        {
            _signinManagerMock.Setup(
                x => x.PasswordSignInAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(Task.FromResult(SignInStatus.Success));

            var model = new InternalLoginViewModel
            {
                Email = "email@email.com",
                Password = "Passwors@124#212",
                RememberMe = true,
                ReturnUrl = _testUrl,
            };

            var result = _subject.InternalLogin(model).Result as JsonResult;

            var expectedResult = new JsonResult
            {
                Data = new { Success = true, ReturnUrl = "/" },     // ReturnUrl should always be without hostname
                JsonRequestBehavior = JsonRequestBehavior.DenyGet
            };

            result.ShouldBeEquivalentTo(expectedResult);
        }

        [TestMethod]
        public void InternalLogin_WhenLockedOut_ShoulReturndLockoutView()
        {
            _signinManagerMock.Setup(
                x => x.PasswordSignInAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(Task.FromResult(SignInStatus.LockedOut));

            var model = new InternalLoginViewModel
            {
                Email = "email@email.com",
                Password = "Passwors@124#212",
                RememberMe = true,
                ReturnUrl = _testUrl,
            };

            var result = _subject.InternalLogin(model).Result as PartialViewResult;

            result.ViewName.Should().Be("Lockout");
        }

        [TestMethod]
        public void InternalLogin_WhenFailure_ShouldReturnModelErrors()
        {
            _signinManagerMock.Setup(
                x => x.PasswordSignInAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(Task.FromResult(SignInStatus.Failure));

            var model = new InternalLoginViewModel
            {
                Email = "email@email.com",
                Password = "Passwors@124#212",
                RememberMe = true,
                ReturnUrl = _testUrl,
            };

            var result = _subject.InternalLogin(model);

            _subject.ModelState.Values.First().Errors.First().ErrorMessage.Should().Be("WrongPasswordOrEmail");
        }

        [TestMethod]
        public void ExternalLogin_WhenRequestUrl_ShouldReturnChallengeResult()
        {
            _subject.Url = new UrlHelper(new RequestContext(_httpContextMock.Object, new RouteData()), new RouteCollection());

            var result = _subject.ExternalLogin("mark", _testUrl) as ChallengeResult;

            var expectedResult = new ChallengeResult("mark", null);

            result.ShouldBeEquivalentTo(expectedResult);
        }

        [TestMethod]
        public void ExternalLoginCallback_WhenSuccessful_ShouldRedirectToReturnUrl()
        {
            _userServiceMock.Setup(
                x => x.GetExternalLoginInfoAsync())
                .Returns(Task.FromResult(new ExternalLoginInfo()));

            _userManagerMock.Setup(
                x => x.FindAsync(It.IsAny<UserLoginInfo>()))
                .Returns(Task.FromResult(new ApplicationUser()));

            _userManagerMock.Setup(
                x => x.IsLockedOutAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(false));

            var result = _subject.ExternalLoginCallback("http://test.com/redirect").Result as RedirectResult;

            result.Url.Should().Be("http://test.com/redirect");
        }

        [TestMethod]
        public void ExternalLoginCallback_WhenLockedOut_ShoulReturndLockoutView()
        {
            _userServiceMock.Setup(
               x => x.GetExternalLoginInfoAsync())
               .Returns(Task.FromResult(new ExternalLoginInfo()));

            _userManagerMock.Setup(
                x => x.FindAsync(It.IsAny<UserLoginInfo>()))
                .Returns(Task.FromResult(new ApplicationUser()));

            _userManagerMock.Setup(
                x => x.IsLockedOutAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(true));

            var result = _subject.ExternalLoginCallback("http://test.com/redirect").Result as RedirectToRouteResult;

            result.RouteValues["action"].Should().Be("Lockout");
        }

        [TestMethod]
        public void ExternalLoginCallback_WhenFailure_ShouldCreateExternalLoginConfirmationViewModel()
        {
            _userServiceMock.Setup(
               x => x.GetExternalLoginInfoAsync())
               .Returns(Task.FromResult(new ExternalLoginInfo
               {
                   Login = new UserLoginInfo("provider", "key")
               }));

            _userManagerMock.Setup(
                x => x.FindAsync(It.IsAny<UserLoginInfo>()))
                .ReturnsAsync((null));

            var result = ((ViewResult)_subject.ExternalLoginCallback("http://test.com/redirect").Result).Model as ExternalLoginConfirmationViewModel;

            var expectedResult = new ExternalLoginConfirmationViewModel
            {
                ReturnUrl = "http://test.com/redirect"
            };
            result.ShouldBeEquivalentTo(expectedResult);
        }

        private LoginControllerForTest _subject;
        private Mock<IContentLoader> _contentLoaderMock;
        private Mock<ApplicationUserManager> _userManagerMock;
        private Mock<UserService> _userServiceMock;
        private Mock<ApplicationSignInManager> _signinManagerMock;
        private Mock<HttpContextBase> _httpContextMock;
        private Mock<ControllerExceptionHandler> _controllerExceptionHandler;
        private Mock<RequestContext> _requestContext;
        private ExceptionContext _exceptionContext;
        private CultureInfo _cultureInfo;
        private const string _testUrl = "http://test.com";

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
            localizationService.AddString(english, "/Login/Form/Error/WrongPasswordOrEmail", "WrongPasswordOrEmail");
            localizationService.AddString(english, "/Shared/Address/DefaultAddressName", "Default address");

            var startPageMock = new Mock<StartPage>();
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var authenticationManager = new Mock<IAuthenticationManager>();

            var customercontextFacadeMock = new Mock<CustomerContextFacade>();
            var countryManagerFacadeMock = new Mock<CountryManagerFacade>();
            countryManagerFacadeMock.Setup(x => x.GetCountries()).Returns(() => new CountryDto());
            var addressBookService = new AddressBookService(customercontextFacadeMock.Object, countryManagerFacadeMock.Object);
            var request = new Mock<HttpRequestBase>();
            _httpContextMock = new Mock<HttpContextBase>();
            _requestContext = new Mock<RequestContext>();
            _controllerExceptionHandler = new Mock<ControllerExceptionHandler>();

            _contentLoaderMock = new Mock<IContentLoader>();
            _userManagerMock = new Mock<ApplicationUserManager>(userStore.Object);
            _signinManagerMock = new Mock<ApplicationSignInManager>(_userManagerMock.Object, authenticationManager.Object);
            _userServiceMock = new Mock<UserService>(_userManagerMock.Object, _signinManagerMock.Object, authenticationManager.Object, localizationService, customercontextFacadeMock.Object);

            request.Setup(
                x => x.Url)
                .Returns(new Uri(_testUrl));

            request.SetupGet(
                x => x.UrlReferrer)
                .Returns(new Uri(_testUrl));

            _httpContextMock.SetupGet(
                x => x.Request)
                .Returns(request.Object);

            _contentLoaderMock.Setup(x => x.Get<StartPage>(It.IsAny<ContentReference>())).Returns(startPageMock.Object);

            _subject = new LoginControllerForTest(_signinManagerMock.Object, _userManagerMock.Object, _userServiceMock.Object, localizationService, _contentLoaderMock.Object, addressBookService, _controllerExceptionHandler.Object);
            _subject.ControllerContext = new ControllerContext(_httpContextMock.Object, new RouteData(), _subject);

            _exceptionContext = new ExceptionContext
            {
                HttpContext = _httpContextMock.Object,
                RequestContext = _requestContext.Object
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            Thread.CurrentThread.CurrentUICulture = _cultureInfo;
        }

        private void Setup_exception(Exception exception)
        {
            _exceptionContext.Exception = exception;
        }

        private void Setup_action_for_controller(string actionName)
        {
            var routeData = new RouteData();
            routeData.Values.Add("action", actionName);

            _exceptionContext.RouteData = routeData;
        }

        private class LoginControllerForTest : LoginController
        {
            public LoginControllerForTest(ApplicationSignInManager signInManager, ApplicationUserManager userManager, UserService userService, LocalizationService localizationService, IContentLoader contentLoader, IAddressBookService addressBookService, ControllerExceptionHandler controllerExceptionHandler)
                : base(signInManager, userManager, userService, localizationService, contentLoader, addressBookService, controllerExceptionHandler)
            {
            }

            public void CallOnException(ExceptionContext filterContext)
            {
                OnException(filterContext);
            }
        }
    }
}
