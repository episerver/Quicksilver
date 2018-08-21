using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Commerce.Security;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Logging;
using EPiServer.Reference.Commerce.Shared.Identity;
using EPiServer.Reference.Commerce.Shared.Models;
using EPiServer.Reference.Commerce.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Login.Pages;
using EPiServer.Reference.Commerce.Site.Features.Login.Services;
using EPiServer.Reference.Commerce.Site.Features.Login.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using EPiServer.Security;
using Mediachase.Commerce.Customers;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Login.Controllers
{
    public class LoginController : IdentityControllerBase<LoginRegistrationPage>
    {
        private readonly IAddressBookService _addressBookService;
        private readonly IContentLoader _contentLoader;
        private readonly ILogger _logger = LogManager.GetLogger(typeof(LoginController));
        private readonly IMailService _mailService;
        private readonly LocalizationService _localizationService;
        private readonly ControllerExceptionHandler _controllerExceptionHandler;
        private readonly OptinService _optinService;

        private StartPage StartPage => _contentLoader.Get<StartPage>(ContentReference.StartPage);

        public LoginController(
            ApplicationSignInManager<SiteUser> signinManager,
            ApplicationUserManager<SiteUser> userManager,
            UserService userService,
            IAddressBookService addressBookService,
            IContentLoader contentLoader,
            IMailService mailService,
            LocalizationService localizationService,
            ControllerExceptionHandler controllerExceptionHandler,
            OptinService optinService)
            : base(signinManager, userManager, userService)
        {
            _addressBookService = addressBookService;
            _contentLoader = contentLoader;
            _mailService = mailService;
            _localizationService = localizationService;
            _controllerExceptionHandler = controllerExceptionHandler;
            _optinService = optinService;
        }

        /// <summary>
        /// Renders the default login page in which a user can both register a new account or log in
        /// to an existing one.
        /// </summary>
        /// <param name="returnUrl">The user's previous URL location. When logging in the user will be redirected back to this URL.</param>
        /// <returns>The default login and user account registration view.</returns>
        [HttpGet]
        public ActionResult Index(string returnUrl)
        {
            var registrationPage = ContentReference.IsNullOrEmpty(StartPage.LoginRegistrationPage)
                ? new LoginRegistrationPage()
                : _contentLoader.Get<LoginRegistrationPage>(StartPage.LoginRegistrationPage);

            // Prevent open redirection attacks. Refer to: https://docs.microsoft.com/en-us/aspnet/mvc/overview/security/preventing-open-redirection-attacks
            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
            {
                returnUrl = "/";
            }

            var viewModel = new LoginPageViewModel(registrationPage, returnUrl);
            viewModel.LoginViewModel.ResetPasswordPage = StartPage.ResetPasswordPage;

            _addressBookService.LoadAddress(viewModel.RegisterAccountViewModel.Address);
            viewModel.RegisterAccountViewModel.Address.Name = _localizationService.GetString("/Shared/Address/DefaultAddressName");
            
            return View(viewModel);
        }

        [HttpPost]
        [AllowDBWrite]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterAccount(RegisterAccountViewModel viewModel)
        {
            if (viewModel.CurrentPage == null)
            {
                viewModel.CurrentPage = ContentReference.IsNullOrEmpty(StartPage.LoginRegistrationPage)
                ? new LoginRegistrationPage()
                : _contentLoader.Get<LoginRegistrationPage>(StartPage.LoginRegistrationPage);
            }

            if (!ModelState.IsValid)
            {
                _addressBookService.LoadAddress(viewModel.Address);
                return View(viewModel);
            }

            viewModel.Address.BillingDefault = true;
            viewModel.Address.ShippingDefault = true;
            viewModel.Address.Email = viewModel.Email;

            var customerAddress = CustomerAddress.CreateInstance();
            _addressBookService.MapToAddress(viewModel.Address, customerAddress);

            var user = new SiteUser
            {
                UserName = viewModel.Email,
                Email = viewModel.Email,
                Password = viewModel.Password,
                FirstName = viewModel.Address.FirstName,
                LastName = viewModel.Address.LastName,
                RegistrationSource = "Registration page",
                AcceptMarketingEmail = viewModel.AcceptMarketingEmail,
                Addresses = new List<CustomerAddress>(new[] { customerAddress }),
                IsApproved = true
            };

            var registration = await UserService.RegisterAccount(user);

            if (registration.Result.Succeeded)
            {
                if (user.AcceptMarketingEmail)
                {
                    var token = await _optinService.CreateOptinTokenData(user.Id);
                    SendMarketingEmailConfirmationMail(user.Id, registration.Contact, token);
                }

                var returnUrl = GetSafeReturnUrl(Request.UrlReferrer);
                return Json(new { ReturnUrl = returnUrl }, JsonRequestBehavior.DenyGet);
            }

            _addressBookService.LoadAddress(viewModel.Address);

            AddErrors(registration.Result.Errors);

            return PartialView("RegisterAccount", viewModel);
        }

        private string GetSafeReturnUrl(Uri referrer)
        {
            //Make sure we only return to relative url.
            var returnUrl = HttpUtility.ParseQueryString(referrer.Query)["returnUrl"];

            // Prevent open redirection attacks. Refer to: https://docs.microsoft.com/en-us/aspnet/mvc/overview/security/preventing-open-redirection-attacks
            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
            {
                return "/";
            }

            Uri uri;

            return Uri.TryCreate(returnUrl, UriKind.Absolute, out uri) ? uri.PathAndQuery : returnUrl;
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            _controllerExceptionHandler.HandleRequestValidationException(filterContext, "registeraccount", OnRegisterException);
        }

        public ActionResult OnRegisterException(ExceptionContext filterContext)
        {
            var viewModel = new RegisterAccountViewModel
            {
                ErrorMessage = filterContext.Exception.Message,
                Address = new Shared.Models.AddressModel()
            };

            _addressBookService.LoadAddress(viewModel.Address);
            viewModel.Address.Name = _localizationService.GetString("/Shared/Address/DefaultAddressName");

            return View("RegisterAccount", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> InternalLogin(LoginViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.ResetPasswordPage = StartPage.ResetPasswordPage;
                return PartialView("Login", viewModel);
            }

            var user = UserService.GetUser(viewModel.Email);
            if (user == null)
            {
                ModelState.AddModelError("Password", _localizationService.GetString("/Login/Form/Error/WrongPasswordOrEmail"));
                viewModel.Password = null;

                return PartialView("Login", viewModel);
            }

            if (!user.IsApproved)
            {
                return PartialView("Unapproved", viewModel);
            }

            var result = await SignInManager.PasswordSignInAsync(viewModel.Email, viewModel.Password, viewModel.RememberMe, shouldLockout: true);

            switch (result)
            {
                case SignInStatus.Success:
                    break;

                case SignInStatus.LockedOut:
                    return PartialView("Lockout", viewModel);

                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", "Login", new { ReturnUrl = viewModel.ReturnUrl, RememberMe = viewModel.RememberMe });

                default:
                    ModelState.AddModelError("Password", _localizationService.GetString("/Login/Form/Error/WrongPasswordOrEmail"));
                    viewModel.Password = null;

                    return PartialView("Login", viewModel);
            }

            return Json(new { Success = true, ReturnUrl = GetSafeReturnUrl(Request.UrlReferrer) }, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", new { returnUrl = returnUrl }));
        }

        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await UserService.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Index");
            }

            if (await LoginIfExternalProviderAlreadyAssignedAsync() && User.Identity.IsAuthenticated)
            {
                return RedirectToLocal(returnUrl);
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            var user = UserService.GetUser(loginInfo.Email);
            if (user == null || !user.IsApproved)
            {
                result = SignInStatus.Failure;
            }
            else if (user.IsLockedOut)
            {
                result = SignInStatus.LockedOut;
            }
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);

                case SignInStatus.LockedOut:
                    return RedirectToAction("Lockout", "Login");

                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", "Login", new { ReturnUrl = returnUrl, RememberMe = false });

                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;

                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { ReturnUrl = returnUrl });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // If the user aleady has an account with any other provider, log them in.
                if (await LoginIfExternalProviderAlreadyAssignedAsync() && User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Index", "Start");
                }

                // Get the information about the user from the external login provider
                var socialLoginDetails = await UserService.GetExternalLoginInfoAsync();
                if (socialLoginDetails == null)
                {
                    return View("ExternalLoginFailure");
                }

                string firstName = null;
                string lastName = null;
                var nameClaim = socialLoginDetails.ExternalIdentity.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
                if (nameClaim != null)
                {
                    string[] names = nameClaim.Value.Split(' ');
                    firstName = names[0];
                    lastName = names.Length > 1 ? names[1] : string.Empty;
                }
                string eMail = socialLoginDetails.Email;

                var customerAddress = CustomerAddress.CreateInstance();
                customerAddress.Line1 = viewModel.Address;
                customerAddress.PostalCode = viewModel.PostalCode;
                customerAddress.City = viewModel.City;
                customerAddress.CountryName = viewModel.Country;

                var user = new SiteUser
                {
                    UserName = eMail,
                    Email = eMail,
                    FirstName = firstName,
                    LastName = lastName,
                    RegistrationSource = "Social login",
                    AcceptMarketingEmail = viewModel.AcceptMarketingEmail,
                    IsApproved = true
                };

                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    user.Addresses = new List<CustomerAddress>(new[]
                    {
                        customerAddress
                    });
                    var contact = UserService.CreateCustomerContact(user);
                    if (user.AcceptMarketingEmail)
                    {
                        var token = await _optinService.CreateOptinTokenData(user.Id);
                        SendMarketingEmailConfirmationMail(user.Id, contact, token);
                    }

                    result = await UserManager.AddLoginAsync(user.Id, socialLoginDetails.Login);
                    if (result.Succeeded)
                    {
                        return RedirectToLocal(viewModel.ReturnUrl);
                    }
                }

                AddErrors(result.Errors);
            }

            return View(viewModel);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmOptinToken(string userId, string token)
        {
            var confirmResult = await _optinService.ConfirmOptinToken(userId, token);
            if (confirmResult)
            {
                return RedirectToAction("SuccessOptinConfirmation", "StandardPage");
            }

            return RedirectToAction("PageNotFound", "ErrorHandling");
        }

        protected virtual void SendMarketingEmailConfirmationMail(string userId, CustomerContact contact, string token)
        {
            var optinConfirmEmailUrl = Url.Action("ConfirmOptinToken", "Login", new { userId, token }, protocol: Request.Url.Scheme);
            try
            {
                var confirmationMailTitle = _contentLoader.Get<MailBasePage>(StartPage.RegistrationConfirmationMail).MailTitle;
                var confirmationMailBody = _mailService.GetHtmlBodyForMail(StartPage.RegistrationConfirmationMail, new NameValueCollection(), StartPage.Language.Name);
                confirmationMailBody = confirmationMailBody.Replace("[OptinConfirmEmailUrl]", optinConfirmEmailUrl);
                confirmationMailBody = confirmationMailBody.Replace("[Customer]", contact.LastName ?? contact.Email);
                _mailService.Send(confirmationMailTitle, confirmationMailBody, contact.Email);
            }
            catch (Exception e)
            {
                _logger.Warning($"Unable to send marketing email confirmation to '{contact.Email}'.", e);
            }
        }

        private async Task<bool> LoginIfExternalProviderAlreadyAssignedAsync()
        {
            var info = await UserService.GetExternalLoginInfoAsync();

            if (info?.Email == null)
            {
                return false;
            }

            if (PrincipalInfo.CurrentPrincipal.IsInRole(RoleNames.CommerceAdmins) ||
            PrincipalInfo.CurrentPrincipal.IsInRole(RoleNames.CatalogManagers) ||
            PrincipalInfo.CurrentPrincipal.IsInRole(RoleNames.ReportManagers) ||
            PrincipalInfo.CurrentPrincipal.IsInRole(RoleNames.CustomerServiceRepresentatives) ||
            PrincipalInfo.CurrentPrincipal.IsInRole(RoleNames.MarketingManagers))
            {
                return true;
            }

            var user = await UserManager.FindByEmailAsync(info.Email);
            return user != null;
        }
    }
}