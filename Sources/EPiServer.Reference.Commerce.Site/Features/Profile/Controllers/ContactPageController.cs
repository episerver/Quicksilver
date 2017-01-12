using EPiServer.Framework.DataAnnotations;
using EPiServer.Reference.Commerce.Site.Features.Profile.Pages;
using EPiServer.Web.Mvc;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Profile.Controllers
{
    [TemplateDescriptor(Inherited = true)]
    public class ContactPageController : PageController<ContactPage>
    {
        public ActionResult Index(ContactPage currentPage)
        {
            return View(currentPage);
        }
    }
}