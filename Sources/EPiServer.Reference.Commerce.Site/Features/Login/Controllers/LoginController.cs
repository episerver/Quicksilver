using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Commerce.Security;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Shared.Identity;
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
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Login.Controllers
{
    public class LoginController : IdentityControllerBase<LoginRegistrationPage>
    {
        private readonly IContentLoader _contentLoader;
        private readonly IAddressBookService _addressBookService;
        private readonly LocalizationService _localizationService;
        private readonly ControllerExceptionHandler _controllerExceptionHandler;

        public LoginController(
            ApplicationSignInManager<SiteUser> signinManager,
            ApplicationUserManager<SiteUser> userManager,
            UserService userService,
            LocalizationService localizationService,
            IContentLoader contentLoader,
            IAddressBookService addressBookService,
            ControllerExceptionHandler controllerExceptionHandler)
            : base(signinManager, userManager, userService)
        {
            _localizationService = localizationService;
            _contentLoader = contentLoader;
            _controllerExceptionHandler = controllerExceptionHandler;
            _addressBookService = addressBookService;
        }

        /// <summary>
        /// Renders the default login page in which a user can both register a new account or log in
        /// to an existing one.
        /// </summary>
        /// <param name="currentPage">An instance of the PageData object for the view.</param>
        /// <param name="returnUrl">The user´s previous URL location. When logging in the user will be redirected back to this URL.</param>
        /// <returns>The default login and user account registration view.</returns>
        [HttpGet]
        public ActionResult Index(LoginRegistrationPage currentPage, string returnUrl)
        {
            var viewModel = new LoginPageViewModel(currentPage, returnUrl ?? "/");
            InitializeLoginViewModel(viewModel.LoginViewModel);
            _addressBookService.LoadAddress(viewModel.RegisterAccountViewModel.Address);
            viewModel.RegisterAccountViewModel.Address.Name = _localizationService.GetString("/Shared/Address/DefaultAddressName");

            return View(viewModel);
        }

        private void InitializeLoginViewModel(LoginViewModel viewModel)
        {
            StartPage startPage = _contentLoader.Get<StartPage>(ContentReference.StartPage);
            viewModel.ResetPasswordPage = startPage.ResetPasswordPage;
        }

        [HttpPost]
        [AllowDBWrite]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterAccount(RegisterAccountViewModel viewModel)
        {
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
                NewsLetter = viewModel.Newsletter,
                Addresses = new List<CustomerAddress>(new[] { customerAddress }),
                IsApproved = true
            };

            var registration = await UserService.RegisterAccount(user);

            if (registration.Result.Succeeded)
            {
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
            if (string.IsNullOrEmpty(returnUrl))
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
            var returnUrl = GetSafeReturnUrl(Request.UrlReferrer);

            if (!ModelState.IsValid)
            {
                InitializeLoginViewModel(viewModel);
                return PartialView("Login", viewModel);
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

            return Json(new { Success = true, ReturnUrl = returnUrl }, JsonRequestBehavior.DenyGet);
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
                    NewsLetter = viewModel.Newsletter,
                    IsApproved = true
                };

                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    user.Addresses = new List<CustomerAddress>(new[]
                    {
                        customerAddress
                    });
                    UserService.CreateCustomerContact(user);
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

        private async Task<bool> LoginIfExternalProviderAlreadyAssignedAsync()
        {
            var info = await UserService.GetExternalLoginInfoAsync();

            if (info?.Email == null)
            {
                return false;
            }

            if (PrincipalInfo.CurrentPrincipal.IsInRole(RoleNames.CommerceAdmins) ||
            PrincipalInfo.CurrentPrincipal.IsInRole(RoleNames.CatalogManagers) ||
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