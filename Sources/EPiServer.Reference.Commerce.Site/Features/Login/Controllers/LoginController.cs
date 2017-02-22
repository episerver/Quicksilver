using EPiServer.Cms.UI.AspNetIdentity;
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
using Mediachase.Commerce.Customers;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
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
        public async Task<ActionResult> RegisterAccount(RegisterAccountViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                _addressBookService.LoadAddress(viewModel.Address);
                return View(viewModel);
            }

            ContactIdentityResult registration = null;
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

            registration = await UserService.RegisterAccount(user);

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

            if (Uri.TryCreate(returnUrl, UriKind.Absolute, out uri))
            {
                return uri.PathAndQuery;
            }
            return returnUrl;

        }

        protected override void OnException(ExceptionContext filterContext)
        {
            _controllerExceptionHandler.HandleRequestValidationException(filterContext, "registeraccount", OnRegisterException);
        }

        public ActionResult OnRegisterException(ExceptionContext filterContext)
        {
            RegisterAccountViewModel viewModel = new RegisterAccountViewModel
            {
                ErrorMessage = filterContext.Exception.Message,
                Address = new Shared.Models.AddressModel()
            };

            _addressBookService.LoadAddress(viewModel.Address);
            viewModel.Address.Name = _localizationService.GetString("/Shared/Address/DefaultAddressName");

            return View("RegisterAccount", viewModel);
        }

        [HttpPost]
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
                return RedirectToAction("Login");
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
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var socialLoginDetails = await UserService.GetExternalLoginInfoAsync();
                if (socialLoginDetails == null)
                {
                    return View("ExternalLoginFailure");
                }

                string eMail = socialLoginDetails.Email;
                string[] names = socialLoginDetails.ExternalIdentity.Name.Split(' ');
                string firstName = names[0];
                string lastName = names.Length > 1 ? names[1] : string.Empty;

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
                    result = await UserManager.AddLoginAsync(user.Id, socialLoginDetails.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(viewModel.ReturnUrl);
                    }
                }

                AddErrors(result.Errors);
            }

            return View(viewModel);
        }
    }
}