using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Core;
using EPiServer.Filters;
using EPiServer.Globalization;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Product.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Product.ViewModelFactories
{
    [ServiceConfiguration(Lifecycle = ServiceInstanceScope.Singleton)]
    public class CatalogEntryViewModelFactory
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
        private readonly LanguageResolver _languageResolver;

        public CatalogEntryViewModelFactory(
            IPromotionService promotionService,
            IContentLoader contentLoader,
            IPriceService priceService,
            ICurrentMarket currentMarket,
            CurrencyService currencyservice,
            IRelationRepository relationRepository,
            AppContextFacade appContext,
            UrlResolver urlResolver,
            FilterPublished filterPublished,
            LanguageResolver languageResolver)
        {
            _promotionService = promotionService;
            _contentLoader = contentLoader;
            _priceService = priceService;
            _currentMarket = currentMarket;
            _currencyservice = currencyservice;
            _relationRepository = relationRepository;
            _appContext = appContext;
            _urlResolver = urlResolver;
            _filterPublished = filterPublished;
            _languageResolver = languageResolver;
        }

        public virtual FashionProductViewModel Create(FashionProduct currentContent, string variationCode)
        {
            var variants = GetVariants(currentContent).ToList();

            FashionVariant variant;
            if (!TryGetFashionVariant(variants, variationCode, out variant))
            {
                return new FashionProductViewModel
                {
                    Product = currentContent,
                    Images = currentContent.GetAssets<IContentImage>(_contentLoader, _urlResolver)
                };
            }

            var market = _currentMarket.GetCurrentMarket();
            var currency = _currencyservice.GetCurrentCurrency();
            var defaultPrice = GetDefaultPrice(variant.Code, market, currency);
            var discountedPrice = GetDiscountPrice(defaultPrice, market, currency);

            var viewModel = new FashionProductViewModel
            {
                Product = currentContent,
                Variant = variant,
                ListingPrice = defaultPrice != null ? defaultPrice.UnitPrice : new Money(0, currency),
                DiscountedPrice = discountedPrice,
                Colors = variants
                    .Where(x => x.Size != null)
                    .GroupBy(x => x.Color)
                    .Select(g => new SelectListItem
                    {
                        Selected = false,
                        Text = g.Key,
                        Value = g.Key
                    })
                    .ToList(),
                Sizes = variants
                    .Where(x => x.Color != null && x.Color.Equals(variant.Color, StringComparison.OrdinalIgnoreCase))
                    .Select(x => new SelectListItem
                    {
                        Selected = false,
                        Text = x.Size,
                        Value = x.Size
                    })
                    .ToList(),
                Color = variant.Color,
                Size = variant.Size,
                Images = variant.GetAssets<IContentImage>(_contentLoader, _urlResolver),
                IsAvailable = defaultPrice != null
            };

            return viewModel;
        }

        public virtual FashionPackageViewModel Create(FashionPackage currentContent)
        {
            var variants = GetVariants(currentContent).ToList();
            var market = _currentMarket.GetCurrentMarket();
            var currency = _currencyservice.GetCurrentCurrency();
            var defaultPrice = GetDefaultPrice(currentContent.Code, market, currency);
            var viewModel = new FashionPackageViewModel
            {
                Package = currentContent,
                ListingPrice = defaultPrice != null ? defaultPrice.UnitPrice : new Money(0, currency),
                DiscountedPrice = GetDiscountPrice(defaultPrice, market, currency),
                Images = currentContent.GetAssets<IContentImage>(_contentLoader, _urlResolver),
                IsAvailable = defaultPrice != null,
                Entries = variants
            };

            return viewModel;
        }

        public virtual FashionBundleViewModel Create(FashionBundle currentContent)
        {
            var variants = GetVariants(currentContent).ToList();
            var viewModel = new FashionBundleViewModel
            {
                Bundle = currentContent,
                Images = currentContent.GetAssets<IContentImage>(_contentLoader, _urlResolver),
                Entries = variants
            };

            return viewModel;
        }

        public virtual FashionVariant SelectVariant(FashionProduct currentContent, string color, string size)
        {
            var variants = GetVariants(currentContent);

            FashionVariant variant;
            if (TryGetFashionVariantByColorAndSize(variants, color, size, out variant)
                || TryGetFashionVariantByColorAndSize(variants, color, string.Empty, out variant))//if we cannot find variation with exactly both color and size then we will try to get variant by color only
            {
                return variant;
            }

            return null;
        }

        private IEnumerable<FashionVariant> GetVariants(PackageContent currentContent)
        {
            return _contentLoader
                .GetItems(currentContent.GetEntries(_relationRepository), _languageResolver.GetPreferredCulture())
                .OfType<FashionVariant>()
                .Where(v => v.IsAvailableInCurrentMarket(_currentMarket) && !_filterPublished.ShouldFilter(v))
                .ToArray();
        }

        private IEnumerable<FashionVariant> GetVariants(FashionBundle currentContent)
        {
            return _contentLoader
                .GetItems(currentContent.GetEntries(_relationRepository), _languageResolver.GetPreferredCulture())
                .OfType<FashionVariant>()
                .Where(v => v.IsAvailableInCurrentMarket(_currentMarket) && !_filterPublished.ShouldFilter(v))
                .ToArray();
        }

        private IEnumerable<FashionVariant> GetVariants(FashionProduct currentContent)
        {
            return _contentLoader
                .GetItems(currentContent.GetVariants(_relationRepository), _languageResolver.GetPreferredCulture())
                .OfType<FashionVariant>()
                .Where(v => v.IsAvailableInCurrentMarket(_currentMarket) && !_filterPublished.ShouldFilter(v))
                .ToArray();
        }

        private static bool TryGetFashionVariant(IEnumerable<FashionVariant> variations, string variationCode, out FashionVariant variation)
        {
            variation = !string.IsNullOrEmpty(variationCode) ?
                variations.FirstOrDefault(x => x.Code == variationCode) :
                variations.FirstOrDefault();

            return variation != null;
        }

        private IPriceValue GetDefaultPrice(string entryCode, IMarket market, Currency currency)
        {
            return _priceService.GetDefaultPrice(
                market.MarketId,
                DateTime.Now,
                new CatalogKey(_appContext.ApplicationId, entryCode),
                currency);
        }

        private Money? GetDiscountPrice(IPriceValue defaultPrice, IMarket market, Currency currency)
        {
            if (defaultPrice == null)
            {
                return null;
            }

            return _promotionService.GetDiscountPrice(defaultPrice.CatalogKey, market.MarketId, currency).UnitPrice;
        }

        private static bool TryGetFashionVariantByColorAndSize(IEnumerable<FashionVariant> variants, string color, string size, out FashionVariant variant)
        {
            variant = variants.FirstOrDefault(x =>
                (string.IsNullOrEmpty(color) || x.Color.Equals(color, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(size) || x.Size.Equals(size, StringComparison.OrdinalIgnoreCase)));

            return variant != null;
        }
    }
}