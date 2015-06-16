using System.Web.Mvc;
using EPiServer.Reference.Commerce.Site.Features.Market.Models;

namespace EPiServer.Reference.Commerce.Site.Features.Market.Controllers
{
    public class CurrencyController : Controller
    {
        private readonly ICurrencyService _currencyService;

        public CurrencyController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        [ChildActionOnly]
        public ActionResult Index()
        {
            var model = new CurrencyViewModel
            {
                Currencies = _currencyService.GetAvailableCurrencies(),
                CurrentCurrency = _currencyService.GetCurrentCurrency()
            };
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Set(string currencyCode)
        {
            if (!_currencyService.SetCurrentCurrency(currencyCode))
            {
                return new HttpStatusCodeResult(400, "Unsupported");
            }
            return Json(new { returnUrl = Request.UrlReferrer.ToString() });
        }
    }

}