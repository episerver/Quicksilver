using System.Web.Mvc;
using EPiServer.Reference.Commerce.Site.Features.Profile.Pages;
using EPiServer.Web.Mvc;
using EPiServer.Reference.Commerce.Site.Features.Profile.ViewModels;

namespace EPiServer.Reference.Commerce.Site.Features.Profile.Controllers
{
    [Authorize]
    public class ProfilePageController : PageController<ProfilePage>
    {
        public ActionResult Index(ProfilePage currentPage)
        {
            var viewModel = new ProfilePageViewModel { CurrentPage = currentPage };
            return View(viewModel);
        }
    }
}