using System.Web.Mvc;
using EPiServer.Reference.Commerce.Site.Features.Profile.Pages;
using EPiServer.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Profile.Controllers
{
    [Authorize]
    public class ProfilePageController : PageController<ProfilePage>
    {
        public ActionResult Index(ProfilePage currentPage)
        {
            return View(currentPage);
        }
    }
}