using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.ErrorHandling.Models;
using EPiServer.Reference.Commerce.Site.Features.ErrorHandling.Pages;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
using System;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.ErrorHandling.Controllers
{
    public class ErrorHandlingController : PageController<ErrorPage>
    {
        private readonly IContentRepository _contentRepository;
        private readonly UrlResolver _urlResolver;

        public ErrorHandlingController(IContentRepository contentRepository, UrlResolver urlResolver)
        {
            _contentRepository = contentRepository;
            _urlResolver = urlResolver;
        }

        [HttpGet]
        public ActionResult Index(ErrorPage currentPage)
        {
            var model = new ErrorViewModel();
            model.CurrentPage = currentPage;
            return View(model);
        }

        [HttpGet]
        public ActionResult PageNotFound()
        {
            try
            {
                var startpage = _contentRepository.Get<StartPage>(ContentReference.StartPage);
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