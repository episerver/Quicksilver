using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Data.Dynamic;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Shared.Identity;
using EPiServer.Reference.Commerce.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Login.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Login.Services;
using EPiServer.Reference.Commerce.Site.Features.Login.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using FluentAssertions;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders.Dto;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Moq;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Login.Controllers
{
    public class LoginControllerTests : IDisposable
    {
        [Fact]
        public void Index_WhenReturnUrl_ShouldCreateViewModel()
        {
            var result = ((ViewResult)_subject.Index(_testUrl)).Model as LoginPageViewModel;

            Assert.NotNull(result);
        }

        [Fact]
        public void Index_WhenReturnUrl_ShouldPassPageToViewModel()
        {
            var result = ((ViewResult)_subject.Index(_testUrl)).Model as LoginPageViewModel;

            Assert.NotNull(result.CurrentPage);
        }

        [Fact]
        public void Index_WhenReturnUrl_ShouldPassUrlToViewModel()
        {
            var result = ((ViewResult)_subject.Index(_testUrl)).Model as LoginPageViewModel;

            Assert.Equal(_testUrl, result.LoginViewModel.ReturnUrl);
        }

        [Fact]
        public void Index_WhenMaliciousReturnUrl_ShouldNotPassUrlToViewModel()
        {
            _urlHelperMock.Setup(x => x.IsLocalUrl(It.IsAny<string>())).Returns(false);
            var result = ((ViewResult)_subject.Index(_testUrl)).Model as LoginPageViewModel;

            Assert.Equal("/", result.LoginViewModel.ReturnUrl);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RegisterAccount_WhenRegisterSuccessful_ShouldReturnJsonReturnUrl(bool acceptMarketingEmail)
        {
            var identityResult = new IdentityResult();
            typeof(IdentityResult).GetProperty("Succeeded").SetValue(identityResult, true, null);

            _userServiceMock.Setup(
                x => x.RegisterAccount(It.IsAny<SiteUser>()))
                .Returns(Task.FromResult(new ContactIdentityResult
                (
                    identityResult,
                    CustomerContact.CreateInstance()
                )));

            _optinServiceMock.Setup(
                x => x.CreateOptinTokenData(It.IsAny<string>()))
                .Returns(Task.FromResult("test token"));

            var model = new RegisterAccountViewModel
            {
                Email = "email@email.com",
                AcceptMarketingEmail = acceptMarketingEmail,
                Password = "Passwors@124#212",
                Password2 = "Passwors@124#212",
            };

            model.Address = new AddressModel
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

            result.Should().BeEquivalentTo(expectedResult);
            _subject.CallToSendMarketingEmailConfirmationMailMethod.Should().Be(acceptMarketingEmail);
        }

        [Fact]
        public void RegisterAccount_WhenRegisterFails_ShouldReturnModelErrorState()
        {
            _userServiceMock.Setup(
                x => x.RegisterAccount(It.IsAny<SiteUser>()))
                .Returns(Task.FromResult(new ContactIdentityResult
                (
                    new IdentityResult("We have an error"),
                    CustomerContact.CreateInstance()
                )));

            var model = new RegisterAccountViewModel
            {
                Email = "email@email.com",
                AcceptMarketingEmail = true,
                Password = "Passwors@124#212",
                Password2 = "Passwors@124#212",
            };

            model.Address = new AddressModel
            {
                Line1 = "Address",
                City = "City",
                CountryName = "Country",
                FirstName = "Fisrt Name",
                LastName = "Last Name",
                PostalCode = "952595",
            };

            var result = _subject.RegisterAccount(model);

            _subject.ModelState.Values.First().Errors.First().ErrorMessage.Should().Be("We have an error");
        }

        [Fact]
        public void OnException_ShouldDelegateToExceptionHandler()
        {
            _subject.CallOnException(_exceptionContext);
            _controllerExceptionHandler.Verify(x => x.HandleRequestValidationException(_exceptionContext, "registeraccount", _subject.OnRegisterException));
        }

        [Fact]
        public void OnRegisterException_ShouldCreateRegisterAccountViewModel()
        {
            //Setup
            {
                Setup_exception(new HttpRequestValidationException());
                Setup_action_for_controller("registeraccount");
            }

            var result = _subject.OnRegisterException(_exceptionContext);

            Assert.IsType<RegisterAccountViewModel>(((ViewResult)result).Model);
        }

        [Fact]
        public void InternalLogin_WhenSuccessful_ShouldReturnJson()
        {
            _signinManagerMock.Setup(
                x => x.PasswordSignInAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(Task.FromResult(SignInStatus.Success));

            _userServiceMock.Setup(
                x => x.GetUser(It.IsAny<string>()))
                .Returns(new SiteUser { IsApproved = true, IsLockedOut = false });

            var model = new LoginViewModel
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

            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public void InternalLogin_WhenLockedOut_ShoulReturndLockoutView()
        {
            _signinManagerMock.Setup(
                x => x.PasswordSignInAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(Task.FromResult(SignInStatus.LockedOut));

            _userServiceMock.Setup(
                x => x.GetUser(It.IsAny<string>()))
                .Returns(new SiteUser { IsApproved = true, IsLockedOut = false });

            var model = new LoginViewModel
            {
                Email = "email@email.com",
                Password = "Passwors@124#212",
                RememberMe = true,
                ReturnUrl = _testUrl,
            };

            var result = _subject.InternalLogin(model).Result as PartialViewResult;

            result.ViewName.Should().Be("Lockout");
        }

        [Fact]
        public void InternalLogin_WhenFailure_ShouldReturnModelErrors()
        {
            _signinManagerMock.Setup(
                x => x.PasswordSignInAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(Task.FromResult(SignInStatus.Failure));

            var model = new LoginViewModel
            {
                Email = "email@email.com",
                Password = "Passwors@124#212",
                RememberMe = true,
                ReturnUrl = _testUrl,
            };

            var result = _subject.InternalLogin(model);

            _subject.ModelState.Values.First().Errors.First().ErrorMessage.Should().Be("WrongPasswordOrEmail");
        }

        [Fact]
        public void ExternalLogin_WhenRequestUrl_ShouldReturnChallengeResult()
        {
            _subject.Url = new UrlHelper(new RequestContext(_httpContextMock.Object, new RouteData()), new RouteCollection());

            var result = _subject.ExternalLogin("mark", _testUrl) as ChallengeResult;

            var expectedResult = new ChallengeResult("mark", null);

            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public void ExternalLoginCallback_WhenSuccessful_ShouldRedirectToReturnUrl()
        {
            _userServiceMock.Setup(
                x => x.GetExternalLoginInfoAsync())
                .Returns(Task.FromResult(new ExternalLoginInfo()));

            _userServiceMock.Setup(
                x => x.GetUser(It.IsAny<string>()))
                .Returns(new SiteUser { IsApproved = true, IsLockedOut = false });

            _userManagerMock.Setup(
                x => x.FindAsync(It.IsAny<UserLoginInfo>()))
                .Returns(Task.FromResult(new SiteUser()));

            _userManagerMock.Setup(
                x => x.IsLockedOutAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(false));
            
            var result = _subject.ExternalLoginCallback("http://test.com/redirect").Result as RedirectResult;

            result.Url.Should().Be("http://test.com/redirect");
        }

        [Fact]
        public void ExternalLoginCallback_WhenLockedOut_ShoulReturndLockoutView()
        {
            _userServiceMock.Setup(
               x => x.GetExternalLoginInfoAsync())
               .Returns(Task.FromResult(new ExternalLoginInfo()));

            _userServiceMock.Setup(
                x => x.GetUser(It.IsAny<string>()))
                .Returns(new SiteUser { IsApproved = true, IsLockedOut = false });

            _userManagerMock.Setup(
                x => x.FindAsync(It.IsAny<UserLoginInfo>()))
                .Returns(Task.FromResult(new SiteUser()));

            _userManagerMock.Setup(
                x => x.IsLockedOutAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(true));

            var result = _subject.ExternalLoginCallback("http://test.com/redirect").Result as RedirectToRouteResult;

            result.RouteValues["action"].Should().Be("Lockout");
        }

        [Fact]
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
                .Returns(Task.FromResult<SiteUser>(null));

            var result = ((ViewResult)_subject.ExternalLoginCallback("http://test.com/redirect").Result).Model as ExternalLoginConfirmationViewModel;

            var expectedResult = new ExternalLoginConfirmationViewModel
            {
                ReturnUrl = "http://test.com/redirect"
            };
            result.Should().BeEquivalentTo(expectedResult);
        }

        private readonly LoginControllerForTest _subject;
        private readonly Mock<IContentLoader> _contentLoaderMock;
        private readonly Mock<IOrderGroupFactory> _orderGroupFactoryMock;
        private readonly Mock<ApplicationUserManager<SiteUser>> _userManagerMock;
        private readonly Mock<UserService> _userServiceMock;
        private readonly Mock<OptinService> _optinServiceMock;
        private readonly Mock<ApplicationSignInManager<SiteUser>> _signinManagerMock;
        private readonly Mock<HttpContextBase> _httpContextMock;
        private readonly Mock<ControllerExceptionHandler> _controllerExceptionHandler;
        private readonly Mock<RequestContext> _requestContext;
        private readonly ExceptionContext _exceptionContext;
        private readonly CultureInfo _cultureInfo;
        private readonly Mock<IMailService> _mailServiceMock;
        private readonly Mock<UrlHelper> _urlHelperMock;
        private const string _testUrl = "http://test.com";

        public LoginControllerTests()
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
            var userStore = new Mock<IUserStore<SiteUser>>();
            var authenticationManager = new Mock<IAuthenticationManager>();
            _orderGroupFactoryMock = new Mock<IOrderGroupFactory>();

            var customercontextFacadeMock = new Mock<CustomerContextFacade>(null);
            var countryManagerFacadeMock = new Mock<CountryManagerFacade>();
            countryManagerFacadeMock.Setup(x => x.GetCountries()).Returns(() => new CountryDto());
            var addressBookService = new AddressBookService(customercontextFacadeMock.Object, countryManagerFacadeMock.Object, _orderGroupFactoryMock.Object);
            var request = new Mock<HttpRequestBase>();
            _httpContextMock = new Mock<HttpContextBase>();
            _requestContext = new Mock<RequestContext>();
            _controllerExceptionHandler = new Mock<ControllerExceptionHandler>();

            _contentLoaderMock = new Mock<IContentLoader>();
            _userManagerMock = new Mock<ApplicationUserManager<SiteUser>>(userStore.Object);
            _signinManagerMock = new Mock<ApplicationSignInManager<SiteUser>>(_userManagerMock.Object, authenticationManager.Object, new ApplicationOptions());
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

            _mailServiceMock = new Mock<IMailService>();
            _optinServiceMock = new Mock<OptinService>(new Mock<DynamicDataStoreFactory>().Object, _userManagerMock.Object, customercontextFacadeMock.Object);

            _subject = new LoginControllerForTest(_signinManagerMock.Object, _userManagerMock.Object, _userServiceMock.Object, addressBookService, _contentLoaderMock.Object, _mailServiceMock.Object, localizationService, _controllerExceptionHandler.Object, _optinServiceMock.Object);
            _subject.ControllerContext = new ControllerContext(_httpContextMock.Object, new RouteData(), _subject);

            _urlHelperMock = new Mock<UrlHelper>();
            _urlHelperMock.Setup(x => x.IsLocalUrl(It.IsAny<string>())).Returns(true);
            _subject.Url = _urlHelperMock.Object;

            _exceptionContext = new ExceptionContext
            {
                HttpContext = _httpContextMock.Object,
                RequestContext = _requestContext.Object
            };
        }

        public void Dispose()
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
            public bool CallToSendMarketingEmailConfirmationMailMethod { get; set; }

            public LoginControllerForTest(ApplicationSignInManager<SiteUser> signInManager,
                ApplicationUserManager<SiteUser> userManager,
                UserService userService,
                IAddressBookService addressBookService,
                IContentLoader contentLoader,
                IMailService mailService,
                LocalizationService localizationService,
                ControllerExceptionHandler controllerExceptionHandler,
                OptinService optinService)
                : base(signInManager, userManager, userService, addressBookService, contentLoader, mailService, localizationService, controllerExceptionHandler, optinService)
            {
            }

            public void CallOnException(ExceptionContext filterContext)
            {
                OnException(filterContext);
            }

            protected override void SendMarketingEmailConfirmationMail(string userId, CustomerContact contact, string token)
            {
                CallToSendMarketingEmailConfirmationMailMethod = true;
            }
        }
    }
}
