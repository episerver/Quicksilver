using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.ErrorHandling.Pages;
using EPiServer.Reference.Commerce.Site.Features.ErrorHandling.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
using System;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.ErrorHandling.Controllers
{
    public class ErrorHandlingController : PageController<ErrorPage>
    {
        private readonly IContentLoader _contentLoader;
        private readonly UrlResolver _urlResolver;

        public ErrorHandlingController(IContentLoader contentLoader, UrlResolver urlResolver)
        {

            _contentLoader = contentLoader;
            _urlResolver = urlResolver;
        }

        [HttpGet]
        public ActionResult Index(ErrorPage currentPage)
        {
            var model = new ErrorViewModel
            {
                CurrentPage = currentPage
            };
            return View(model);
        }

        [HttpGet]
        public ActionResult PageNotFound()
        {
            try
            {
                var startpage = _contentLoader.Get<StartPage>(ContentReference.StartPage);
                var url = _urlResolver.GetUrl(startpage.PageNotFound);
                return Redirect(url ?? "~/Features/ErrorHandling/Pages/ErrorFallback.html");
            }
            catch (Exception)
            {
                return Redirect("~/Features/ErrorHandling/Pages/ErrorFallback.html");
            }
        }
    }
}