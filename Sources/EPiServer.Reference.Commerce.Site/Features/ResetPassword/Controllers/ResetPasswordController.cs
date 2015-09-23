using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.Editor;
using EPiServer.Reference.Commerce.Shared.Models;
using EPiServer.Reference.Commerce.Shared.Models.Identity;
using EPiServer.Reference.Commerce.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.ResetPassword.Pages;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Web.Mvc;
using EPiServer.Reference.Commerce.Site.Features.Login.Controllers;
using System.Threading.Tasks;
using EPiServer.Reference.Commerce.Site.Features.ResetPassword.ViewModels;
using System.Collections.Specialized;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Controllers;
using Microsoft.Owin;
using EPiServer.Reference.Commerce.Site.Features.Login.Services;

namespace EPiServer.Reference.Commerce.Site.Features.ResetPassword.Controllers
{
    public class ResetPasswordController : IdentityControllerBase<ResetPasswordPage>
    {
        private readonly IContentLoader _contentLoader;
        private readonly IMailService _mailService;

        public ResetPasswordController(ApplicationSignInManager signinManager, ApplicationUserManager userManager, UserService userService, IContentLoader contentLoader, IMailService mailService)
            : base(signinManager, userManager, userService)
        {
            _contentLoader = contentLoader;
            _mailService = mailService;
        }

        [AllowAnonymous]
        public ActionResult Index(ResetPasswordPage currentPage)
        {
            ForgotPasswordViewModel viewModel = new ForgotPasswordViewModel() { CurrentPage = currentPage };
            return View("ForgotPassword", viewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model, string language)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist or is not confirmed
                return View("ForgotPasswordConfirmation");
            }

            var startPage = _contentLoader.Get<StartPage>(ContentReference.StartPage);
            NameValueCollection queryParameters = new NameValueCollection { { "id", user.Id } };
            string body = _mailService.GetHtmlBodyForMail(startPage.ResetPasswordMail, queryParameters, language);
            var mailPage = _contentLoader.Get<MailBasePage>(startPage.ResetPasswordMail);

            await UserManager.SendEmailAsync(user.Id, mailPage.MailTitle, body);

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View("ForgotPasswordConfirmation");
        }

        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            ResetPasswordViewModel viewModel = new ResetPasswordViewModel { Code = code };
            return code == null ? View("Error") : View("ResetPassword", viewModel);
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

            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation");
            }

            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);

            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            AddErrors(result.Errors);

            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View("ResetPasswordConfirmation");
        }
    }
}