using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Reference.Commerce.Shared.Identity;
using EPiServer.Reference.Commerce.Site.Features.Login.Services;
using EPiServer.Reference.Commerce.Site.Features.ResetPassword.Pages;
using EPiServer.Reference.Commerce.Site.Features.ResetPassword.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Controllers;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.ResetPassword.Controllers
{
    public class ResetPasswordMailController : IdentityControllerBase<ResetPasswordMailPage>
    {
        public ResetPasswordMailController(ApplicationSignInManager<SiteUser> signinManager, ApplicationUserManager<SiteUser> userManager, UserService userService)
            : base(signinManager, userManager, userService)
        {
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public async Task<ActionResult> Index(ResetPasswordMailPage currentPage, string language)
        {
            var viewModel = new ResetPasswordMailPageViewModel { CurrentPage = currentPage };
            return await Task.FromResult(View(viewModel));
        }
    }
}