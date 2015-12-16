using EPiServer.Reference.Commerce.Shared.Models.Identity;
using EPiServer.Reference.Commerce.Site.Features.Login.Services;
using EPiServer.Reference.Commerce.Site.Features.ResetPassword.Pages;
using EPiServer.Reference.Commerce.Site.Features.Shared.Controllers;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.ResetPassword.Controllers
{
    public class ResetPasswordMailController : IdentityControllerBase<ResetPasswordMailPage>
    {
        public ResetPasswordMailController(ApplicationSignInManager signinManager, ApplicationUserManager userManager, UserService userService)
            : base(signinManager, userManager, userService)
        {
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public async Task<ActionResult> Index(ResetPasswordMailPage currentPage, string language)
        {
            return await Task.FromResult(View(currentPage));
        }
    }
}