using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Globalization;
using EPiServer.Reference.Commerce.Site.Features.Market;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Web.Mvc;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Pricing;
using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Product.Controllers
{
    public class ProductController : ContentController<FashionProduct>
    {
        private readonly IPricingService _pricingService;
        private readonly IContentLoader _contentLoader;
        private readonly IPriceService _priceService;
        private readonly ICurrentMarket _currentMarket;
        private readonly CultureInfo _preferredCulture;
        private readonly CurrencyService _currencyservice;

        public ProductController(IPricingService pricingService,
                                 IContentLoader contentLoader,
                                 IPriceService priceService,
                                 ICurrentMarket currentMarket,
                                 CurrencyService currencyservice)
        {
            _pricingService = pricingService;
            _contentLoader = contentLoader;
            _priceService = priceService;
            _currentMarket = currentMarket;
            _currencyservice = currencyservice;
            _preferredCulture = ContentLanguage.PreferredCulture;
        }

        [HttpGet]
        public ActionResult Index(FashionProduct currentContent, string variationId = "")
        {
            var variations = _contentLoader.GetItems(currentContent.GetVariants(), _preferredCulture).
                                            Cast<FashionVariant>()
                                           .ToList();

            var variation = !string.IsNullOrEmpty(variationId)
                                ? variations.FirstOrDefault(x => x.Code == variationId)
                                : variations.FirstOrDefault();

            if (variation == null)
                return HttpNotFound();

            var market = _currentMarket.GetCurrentMarket();
            var currency = _currencyservice.GetCurrentCurrency();
            var prices = _priceService.GetPrices(market.MarketId,
                                                 DateTime.Now,
                                                 new CatalogKey(AppContext.Current.ApplicationId, variation.Code),
                                                 new PriceFilter
                                                     {
                                                         Currencies = new[]
                                                             {
                                                                 currency
                                                             }
                                                     }).ToList();
            var discountPrice = new Money(0, currency);
            if (prices.Any())
            {
                discountPrice = _pricingService.GetDiscountPrice(prices.First().CatalogKey, market.MarketId, currency).UnitPrice;
            }
            var model = new FashionProductViewModel
                {
                    Product = currentContent,
                    Variation = variation,
                    OriginalPrice = prices.Any() ? prices.First().UnitPrice : new Money(0, currency),
                    Price = discountPrice,
                    Colors = currentContent.AvailableColors.Select(x => new SelectListItem
                        {
                            Selected = false,
                            Text = x,
                            Value = x
                        }),
                    Sizes = currentContent.AvailableSizes.Select(x => new SelectListItem
                        {
                            Selected = false,
                            Text = x,
                            Value = x
                        }),
                    Color = variation.Color,
                    Size = variation.Size,
                    Images = variation.GetAssets()
                };

            return Request.IsAjaxRequest() ? PartialView(model) : (ActionResult)View(model);
        }

        [HttpPost]
        public ActionResult SelectVariant(FashionProduct currentContent, string color, string size)
        {
            var variations = _contentLoader.GetItems(currentContent.GetVariants(), _preferredCulture).
                                            Cast<FashionVariant>();

            var variation = variations.FirstOrDefault(x =>
                                                      x.Color.Equals(color, StringComparison.OrdinalIgnoreCase) &&
                                                      x.Size.Equals(size, StringComparison.OrdinalIgnoreCase));
            if (variation == null)
                return HttpNotFound();

            return RedirectToAction("Index", new { variationId = variation.Code });
        }
    }
}