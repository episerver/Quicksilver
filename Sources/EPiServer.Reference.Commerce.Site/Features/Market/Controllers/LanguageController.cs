using System;
using System.Globalization;
using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Market.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Web.Routing;

namespace EPiServer.Reference.Commerce.Site.Features.Market.Controllers
{
    public class LanguageController : Controller
    {
        private readonly LanguageService _languageService;
        private readonly UrlResolver _urlResolver;

        public LanguageController(LanguageService languageService, UrlResolver urlResolver)
        {
            _languageService = languageService;
            _urlResolver = urlResolver;
        }

        [ChildActionOnly]
        public ActionResult Index(ContentReference contentLink, string language)
        {
            var model = new LanguageViewModel
            {
                Languages = _languageService.GetAvailableLanguages(),
                CurrentLanguage = String.IsNullOrEmpty(language) ? _languageService.GetCurrentLanguage() : CultureInfo.GetCultureInfo(language),
                ContentLink = contentLink
            };

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Set(string language, ContentReference contentLink)
        {
            if (!_languageService.SetCurrentLanguage(language))
            {
                return new HttpStatusCodeResult(400, "Unsupported");
            }

            var returnUrl = _urlResolver.GetUrl(Request, contentLink, language);
            return Json(new { returnUrl });
        }
    }
}