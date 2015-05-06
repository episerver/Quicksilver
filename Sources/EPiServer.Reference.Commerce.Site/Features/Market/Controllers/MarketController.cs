using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Market.Models;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using System;
using System.Linq;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Market.Controllers
{
    public class MarketController : Controller
    {
        private readonly IMarketService _marketService;
        private readonly ICurrentMarket _currentMarket;

        public MarketController(IMarketService marketService, ICurrentMarket currentMarket)
        {
            _marketService = marketService;
            _currentMarket = currentMarket;
        }

        [ChildActionOnly]
        public ActionResult Index()
        {
            var model = new MarketViewModel
            {
                Markets = _marketService.GetAllMarkets().Where(x => x.IsEnabled).OrderBy(x => x.MarketName),
                CurrentMarket = _currentMarket.GetCurrentMarket()
            };
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Set(string marketId)
        {
            _currentMarket.SetCurrentMarket(new MarketId(marketId));
            var currentMarket = _marketService.GetMarket(new MarketId(marketId));
            var urlResolver = ServiceLocator.Current.GetInstance<UrlResolver>();
            var url = Request.UrlReferrer == null ? "/" : Request.UrlReferrer.PathAndQuery;
            var content = urlResolver.Route(new UrlBuilder(Request.UrlReferrer));
            if (content != null)
            {
                if (content.ContentLink.ID > 0)
                {
                    url = urlResolver.GetUrl(content.ContentLink, currentMarket.DefaultLanguage.Name);
                    if (Request.UrlReferrer != null && !String.IsNullOrEmpty(Request.UrlReferrer.Query))
                    {
                        url += Request.UrlReferrer.Query;
                    }
                }
            }

            return Json(new { returnUrl = url });
        }
    }
}