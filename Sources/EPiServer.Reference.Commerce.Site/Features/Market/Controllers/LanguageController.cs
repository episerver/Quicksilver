using System.Web.Mvc;
using EPiServer.Reference.Commerce.Site.Features.Market.Models;

namespace EPiServer.Reference.Commerce.Site.Features.Market.Controllers
{
    public class LanguageController : Controller
    {
        private readonly LanguageService _languageService;

        public LanguageController(LanguageService languageService)
        {
            _languageService = languageService;
        }

        [ChildActionOnly]
        public ActionResult Index()
        {
            var model = new LanguageViewModel
            {
                Languages = _languageService.GetAvailableLanguages(),
                CurrentLanguage = _languageService.GetCurrentLanguage()
            };
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Set(string language)
        {
            if (!_languageService.SetCurrentLanguage(language))
            {
                return new HttpStatusCodeResult(400, "Unsupported");
            }
            return Json(new { returnUrl = Request.UrlReferrer.ToString() });
        }
    }
}