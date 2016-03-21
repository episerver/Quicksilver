using System.Linq;
using System.Web.Mvc;
using EPiServer.Reference.Commerce.Site.Features.Market.Models;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;

namespace EPiServer.Reference.Commerce.Site.Features.Market.Controllers
{
    public class CurrencyController : Controller
    {
        private readonly ICurrencyService _currencyService;
        private readonly ICartService _cartService;

        public CurrencyController(ICurrencyService currencyService, ICartService cartService)
        {
            _currencyService = currencyService;
            _cartService = cartService;
        }

        [ChildActionOnly]
        public ActionResult Index()
        {
            var model = new CurrencyViewModel
            {
               
                Currencies = _currencyService.GetAvailableCurrencies()
                    .Select(x => new SelectListItem
                {
                    Selected = false,
                    Text = x.CurrencyCode,
                    Value = x.CurrencyCode
                }),
                CurrencyCode = _currencyService.GetCurrentCurrency().CurrencyCode,
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
            
            var currentCurrency = _currencyService.GetCurrentCurrency();
            _cartService.SetCartCurrency(currentCurrency);
            
            return Json(new { returnUrl = Request.UrlReferrer.ToString() });
        }
    }

}