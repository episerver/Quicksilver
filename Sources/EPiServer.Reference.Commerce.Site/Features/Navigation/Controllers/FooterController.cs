using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Navigation.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.SpecializedProperties;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Navigation.Controllers
{
    public class FooterController : Controller
    {
        private readonly IContentLoader _contentLoader;

        public FooterController(IContentLoader contentLoader)
        {
            _contentLoader = contentLoader;
        }

        [ChildActionOnly]
        public ActionResult Index()
        {
            var viewModel = new FooterViewModel
            {
                FooterLinks = _contentLoader.Get<StartPage>(ContentReference.StartPage).FooterLinks ?? new LinkItemCollection()
            };

            return PartialView(viewModel);
        }
    }
}