using System.Web.Mvc;
using EPiServer.Editor;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.Login;
using EPiServer.Reference.Commerce.Site.Features.ResetPassword.Models;
using EPiServer.Reference.Commerce.Site.Features.ResetPassword.Pages;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
using Mediachase.Commerce.Customers;
using EPiServer.Reference.Commerce.Site.Features.Login.Services;
using EPiServer.Reference.Commerce.Site.Features.Login.Controllers;
using System;

namespace EPiServer.Reference.Commerce.Site.Features.ResetPassword.Controllers
{
    public class ResetPasswordLinkController : PageController<PasswordResetLinkPage>
    {
        private readonly IPasswordHashService _passwordHashService;

        public ResetPasswordLinkController(IPasswordHashService passwordHashService)
        {
            _passwordHashService = passwordHashService;
        }

        [HttpGet]
        public ActionResult Index(PasswordResetLinkPage currentPage, string hash)
        {
            var pageRouteHelper = ServiceLocator.Current.GetInstance<PageRouteHelper>();
            var contact = _passwordHashService.DecodeValidateHash(hash);
            if (PageEditing.PageIsInEditMode)
            {
                contact = CustomerContext.Current.CurrentContact;
            }

            var model = new ResetPasswordLinkViewModel
            {
                CurrentBlock = currentPage,
                CurrentPageLink = pageRouteHelper.PageLink,
                FormModel = new ResetPasswordLinkFormModel { Hash = hash },
                Success = false
            };

            if (contact == null)
            {
                model.InvalidHash = true;
                ModelState.AddModelError("FormModel.Error", LocalizationService.Current.GetString("/ResetPassword/Form/Error/ExpiredLink"));
            }
            return View("Index", model);
        }

        [HttpPost]
        public ActionResult Index(PasswordResetLinkPage currentPage, ResetPasswordLinkFormModel formModel)
        {

            // Todo: This entire implementation was previously based on ASP.NET Membership. It should be moved into the
            // LoginController and be replaced with the code using ASP.NET Identity instead.

            throw new NotImplementedException();
        }
    }
}