using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Linq;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Login.Pages;
using EPiServer.Web.Mvc;
using Microsoft.Owin.Security;
using System.Net;
using System.Threading.Tasks;
using EPiServer.Reference.Commerce.Site.Features.Login.Models;
using Microsoft.AspNet.Identity.Owin;
using EPiServer.Reference.Commerce.Site.Features.Login.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.Registration.Models;
using System.Collections.Generic;
using EPiServer.Reference.Commerce.Site.Features.Login.Services;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Mediachase.Commerce.Customers;
using System;

namespace EPiServer.Reference.Commerce.Site.Features.Login.Controllers
{
    /// <summary>
    /// Default controller managing user account logins.
    /// </summary>
    public class LoginController : LoginControllerBase<LoginRegistrationPage>
    {
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
            LoginPageViewModel<LoginRegistrationPage> viewModel = new LoginPageViewModel<LoginRegistrationPage>(currentPage, returnUrl ?? "/");
            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> RegisterAccount(RegisterAccountViewModel viewModel)
        {
            ContactIdentityResult registration = null;

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            CustomerAddress address = CustomerAddress.CreateInstance();
            address.City = viewModel.City;
            address.CountryName = viewModel.Country;
            address.Email = viewModel.Email;
            address.FirstName = viewModel.FirstName;
            address.LastName = viewModel.LastName;
            address.Line1 = viewModel.Address;
            address.PostalCode = viewModel.PostalCode;

            ApplicationUser user = new ApplicationUser
            {
                UserName = viewModel.Email,
                Email = viewModel.Email,
                Password = viewModel.Password,
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                RegistrationSource = "Registration page",
                NewsLetter = viewModel.Newsletter,
                Addresses = new List<CustomerAddress>(new CustomerAddress[] { address })
            };

            registration = await UserService.RegisterAccount(user);

            if (registration.Result.Succeeded)
            {
                var returnUrl = GetSafeReturnUrl(Request.UrlReferrer);
                return Json(new { ReturnUrl = returnUrl }, JsonRequestBehavior.DenyGet);
            }

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
            if(Uri.TryCreate(returnUrl, UriKind.Absolute, out uri))
            {
                return uri.PathAndQuery;
            }
            return returnUrl;

        }

        [HttpPost]
        public async Task<ActionResult> InternalLogin(InternalLoginViewModel viewModel)
        {
            bool successfull = false;
            string returnUrl = GetSafeReturnUrl(Request.UrlReferrer);

            if (!ModelState.IsValid)
            {
                return PartialView("Login", viewModel);
            }

            var result = await SignInManager.PasswordSignInAsync(viewModel.Email, viewModel.Password, viewModel.RememberMe, shouldLockout: true);
            var user = UserService.GetUser(viewModel.Email);

            switch (result)
            {
                case SignInStatus.Success:
                    successfull = true;
                    break;

                case SignInStatus.LockedOut:
                    return View("Lockout", "Login");

                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", "Login", new { ReturnUrl = viewModel.ReturnUrl, RememberMe = viewModel.RememberMe });

                case SignInStatus.Failure:

                default:
                    ModelState.AddModelError("Password", LocalizationService.Current.GetString("/Login/Form/Error/WrongPasswordOrEmail"));
                    viewModel.Password = null;

                    return PartialView("Login", viewModel);
            }

            return Json(new { Success = successfull, ReturnUrl = returnUrl }, JsonRequestBehavior.DenyGet);
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
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model)
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

                var user = new ApplicationUser
                {
                    UserName = eMail,
                    Email = eMail,
                    FirstName = firstName,
                    LastName = lastName,
                    RegistrationSource = "Social login",
                    NewsLetter = model.Newsletter
                };

                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, socialLoginDetails.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(model.ReturnUrl);
                    }
                }

                AddErrors(result.Errors);
            }

            return View(model);
        }

        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();

            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }

            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            ResetPasswordViewModel viewModel = new ResetPasswordViewModel();

            if (code == null)
            {
                return View("Error");
            }

            return View(viewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await UserService.GetUserAsync(model.Email);

            if (user == null)
            {
                // Don't reveal that the user does not exist.
                return RedirectToAction("ResetPasswordConfirmation");
            }

            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);

            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            AddErrors(result.Errors);

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}