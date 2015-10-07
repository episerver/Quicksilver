using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.Editor;
using EPiServer.Reference.Commerce.Shared.Models.Identity;
using EPiServer.Reference.Commerce.Site.Features.ResetPassword.Pages;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Web.Mvc;
using EPiServer.Reference.Commerce.Site.Features.Login.Controllers;
using System.Threading.Tasks;
using EPiServer.Reference.Commerce.Site.Features.ResetPassword.ViewModels;
using EPiServer.Web.Routing;
using EPiServer.Reference.Commerce.Site.Features.Shared.Controllers;
using Microsoft.Owin;
using EPiServer.Reference.Commerce.Site.Features.Login.Services;

namespace EPiServer.Reference.Commerce.Site.Features.ResetPassword.Controllers
{
    public class ResetPasswordMailController : IdentityControllerBase<ResetPasswordMailPage>
    {
        public ResetPasswordMailController(ApplicationSignInManager signinManager, ApplicationUserManager userManager, UserService userService)
            : base(signinManager, userManager, userService)
        {
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> Index(ResetPasswordMailPage currentPage, string id, string language)
        {
            ResetPasswordMailModel viewModel = new ResetPasswordMailModel();

            if (id != null)
            {
                string code = await UserManager.GeneratePasswordResetTokenAsync(id);
                viewModel.CallbackUrl = Url.Action("ResetPassword", "ResetPassword", new { userId = id, code = code, langauge = language }, protocol: Request.Url.Scheme);
            }

            return View(viewModel);
        }
    }
}