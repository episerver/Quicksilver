using System.Web.Mvc;
using EPiServer.Reference.Commerce.Site.Features.Campaign.Pages;
using EPiServer.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Campaign.Controllers
{
    public class CampaignController : PageController<CampaignPage>
    {
        [HttpGet]
        public ActionResult Index(CampaignPage currentPage)
        {
            return View(currentPage);
        }
    }
}