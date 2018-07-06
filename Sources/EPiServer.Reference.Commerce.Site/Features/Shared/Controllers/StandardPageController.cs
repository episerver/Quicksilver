using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Shared.Pages;
using EPiServer.Reference.Commerce.Site.Features.Shared.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
using System;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Controllers
{
    public class StandardPageController : PageController<StandardPage>
    {
        private readonly IContentLoader _contentLoader;
        private readonly UrlResolver _urlResolver;

        public StandardPageController(IContentLoader contentLoader, UrlResolver urlResolver)
        {

            _contentLoader = contentLoader;
            _urlResolver = urlResolver;
        }

        [HttpGet]
        public ActionResult Index(StandardPage currentPage)
        {
            var model = new StandardPageViewModel
            {
                CurrentPage = currentPage
            };
            return View(model);
        }

        [HttpGet]
        public ActionResult SuccessOptinConfirmation()
        {
            try
            {
                var startpage = _contentLoader.Get<StartPage>(ContentReference.StartPage);
                var url = _urlResolver.GetUrl(startpage.OptinConfirmSuccessPage);
                return Redirect(url ?? "~/Features/ErrorHandling/Pages/ErrorFallback.html");
            }
            catch (Exception)
            {
                return Redirect("~/Features/ErrorHandling/Pages/ErrorFallback.html");
            }
        }
    }
}