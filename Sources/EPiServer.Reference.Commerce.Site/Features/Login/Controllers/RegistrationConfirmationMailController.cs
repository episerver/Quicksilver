using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Reference.Commerce.Shared.Identity;
using EPiServer.Reference.Commerce.Site.Features.Login.Pages;
using EPiServer.Reference.Commerce.Site.Features.Login.Services;
using EPiServer.Reference.Commerce.Site.Features.Login.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Controllers;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Login.Controllers
{
    public class RegistrationConfirmationMailController : IdentityControllerBase<RegistrationConfirmationMailPage>
    {
        public RegistrationConfirmationMailController(
            ApplicationSignInManager<SiteUser> signinManager,
            ApplicationUserManager<SiteUser> userManager,
            UserService userService) : base(signinManager, userManager, userService) { }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Index(RegistrationConfirmationMailPage currentPage, string language)
        {
            var viewModel = new RegistrationConfirmationMailPageViewModel { CurrentPage = currentPage };
            return View(viewModel);
        }
    }
}