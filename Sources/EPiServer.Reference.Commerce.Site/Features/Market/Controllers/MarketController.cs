using System.Linq;
using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Market.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;

namespace EPiServer.Reference.Commerce.Site.Features.Market.Controllers
{
    public class MarketController : Controller
    {
        private readonly IMarketService _marketService;
        private readonly ICurrentMarket _currentMarket;
        private readonly UrlResolver _urlResolver;
        private readonly LanguageService _languageService;

        public MarketController(IMarketService marketService, ICurrentMarket currentMarket, UrlResolver urlResolver, LanguageService languageService)
        {
            _marketService = marketService;
            _currentMarket = currentMarket;
            _urlResolver = urlResolver;
            _languageService = languageService;
        }

        [ChildActionOnly]
        public ActionResult Index(ContentReference contentLink)
        {
            var model = new MarketViewModel
            {
                Markets = _marketService.GetAllMarkets().Where(x => x.IsEnabled).OrderBy(x => x.MarketName)
                    .Select(x => new SelectListItem
                {
                    Selected = false,
                    Text = x.MarketName,
                    Value = x.MarketId.Value
                }),
                MarketId = _currentMarket.GetCurrentMarket().MarketId.Value,
                ContentLink = contentLink
            };
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Set(string marketId, ContentReference contentLink)
        {
            _currentMarket.SetCurrentMarket(new MarketId(marketId));
            var currentMarket = _marketService.GetMarket(new MarketId(marketId));
            _languageService.SetCurrentLanguage(currentMarket.DefaultLanguage.Name);

            var returnUrl = _urlResolver.GetUrl(Request, contentLink, currentMarket.DefaultLanguage.Name);       
            return Json(new { returnUrl });
        }
    }
}