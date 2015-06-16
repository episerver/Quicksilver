using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Core;
using EPiServer.Filters;
using EPiServer.Reference.Commerce.Site.Features.Market;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Pricing;

namespace EPiServer.Reference.Commerce.Site.Features.Product.Controllers
{
    public class ProductController : ContentController<FashionProduct>
    {
        private readonly IPromotionService _promotionService;
        private readonly IContentLoader _contentLoader;
        private readonly IPriceService _priceService;
        private readonly ICurrentMarket _currentMarket;
        private readonly ICurrencyService _currencyservice;
        private readonly IRelationRepository _relationRepository;
        private readonly AppContextFacade _appContext;
        private readonly UrlResolver _urlResolver;
        private readonly FilterPublished _filterPublished;
        private readonly CultureInfo _preferredCulture;

        public ProductController(
            IPromotionService promotionService,
            IContentLoader contentLoader,
            IPriceService priceService,
            ICurrentMarket currentMarket,
            CurrencyService currencyservice, 
            IRelationRepository relationRepository, 
            AppContextFacade appContext, 
            UrlResolver urlResolver,
            FilterPublished filterPublished,
            Func<CultureInfo> preferredCulture)
        {
            _promotionService = promotionService;
            _contentLoader = contentLoader;
            _priceService = priceService;
            _currentMarket = currentMarket;
            _currencyservice = currencyservice;
            _relationRepository = relationRepository;
            _appContext = appContext;
            _urlResolver = urlResolver;
            _preferredCulture = preferredCulture();
            _filterPublished = filterPublished;
        }

        [HttpGet]
        public ActionResult Index(FashionProduct currentContent, string variationCode = "")
        {
            FashionVariant variation;
            if (!TryGetFashionVariant(currentContent, variationCode, out variation))
            {
                return HttpNotFound();
            }

            var market = _currentMarket.GetCurrentMarket();
            var currency = _currencyservice.GetCurrentCurrency();

            var defaultPrice = GetDefaultPrice(variation, market, currency);
            var discountPrice = GetDiscountPrice(defaultPrice, market, currency);

            var model = new FashionProductViewModel
            {
                Product = currentContent,
                Variation = variation,
                OriginalPrice = defaultPrice != null ? defaultPrice.UnitPrice : new Money(0, currency),
                Price = discountPrice,
                Colors = currentContent.AvailableColors.Select(x => new SelectListItem
                {
                    Selected = false,
                    Text = x,
                    Value = x
                }),
                Sizes = _contentLoader
                    .GetItems(currentContent.GetVariants(_relationRepository), _preferredCulture)
                    .Cast<FashionVariant>()
                    .ToList()
                    .Where(x => x.Color.Equals(variation.Color))
                    .Select(x => new SelectListItem
                {
                    Selected = false,
                    Text = x.Size,
                    Value = x.Size
                }),
                Color = variation.Color,
                Size = variation.Size,
                Images = variation.GetAssets<IContentImage>(_contentLoader, _urlResolver)
            };

            return Request.IsAjaxRequest() ? PartialView(model) : (ActionResult)View(model);
        }

        [HttpPost]
        public ActionResult SelectVariant(FashionProduct currentContent, string color)
        {
            FashionVariant variation;
            if (!TryGetFashionVariantByColor(currentContent, color, out variation))
            {
                return HttpNotFound();
            }

            return RedirectToAction("Index", new { variationCode = variation.Code });
        }

        private bool TryGetFashionVariant(FashionProduct currentContent, string variationCode, out FashionVariant variation)
        {
            return TryGetFashionVariant(currentContent, (variations) => !string.IsNullOrEmpty(variationCode)
                ? variations.FirstOrDefault(x => x.Code == variationCode)
                : variations.FirstOrDefault(), out variation);
        }

        private bool TryGetFashionVariantByColor(FashionProduct currentContent, string color, out FashionVariant variation)
        {
            return TryGetFashionVariant(currentContent, (variations) => variations.FirstOrDefault(x =>
                x.Color.Equals(color, StringComparison.OrdinalIgnoreCase)), out variation);
        }

        private bool TryGetFashionVariant(FashionProduct currentContent, Func<IEnumerable<FashionVariant>, FashionVariant> selectVariation, out FashionVariant variation)
        {
            var variations = _contentLoader
                .GetItems(currentContent.GetVariants(_relationRepository), _preferredCulture)
                .Cast<FashionVariant>()
                .Where(v => v.IsAvailableInCurrentMarket(_currentMarket) && !_filterPublished.ShouldFilter(v))
                .ToList();
            
            variation = selectVariation(variations);
            return variation != null;
        }

        private IPriceValue GetDefaultPrice(FashionVariant variation, IMarket market, Currency currency)
        {
            return _priceService.GetDefaultPrice(
                market.MarketId,
                DateTime.Now,
                new CatalogKey(_appContext.ApplicationId, variation.Code),
                currency);
        }

        private Money GetDiscountPrice(IPriceValue defaultPrice, IMarket market, Currency currency)
        {
            if (defaultPrice == null)
            {
                return new Money(0, currency);
            }

            return _promotionService.GetDiscountPrice(defaultPrice.CatalogKey, market.MarketId, currency).UnitPrice;
        }
    }
}