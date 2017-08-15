using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Product.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Product.ViewModelFactories
{
    [ServiceConfiguration(Lifecycle = ServiceInstanceScope.Singleton)]
    public class CatalogEntryViewModelFactory
    {
        private readonly IContentLoader _contentLoader;
        private readonly IPricingService _pricingService;
        private readonly UrlResolver _urlResolver;
        private readonly CatalogContentService _catalogContentService;

        public CatalogEntryViewModelFactory(
            IContentLoader contentLoader,
            IPricingService pricingService,
            UrlResolver urlResolver,
            CatalogContentService catalogContentService)
        {
            _contentLoader = contentLoader;
            _pricingService = pricingService;
            _urlResolver = urlResolver;
            _catalogContentService = catalogContentService;
        }

        public virtual FashionProductViewModel Create(FashionProduct currentContent, string variationCode)
        {
            var variants = _catalogContentService.GetVariants<FashionVariant>(currentContent).ToList();

            FashionVariant variant;
            if (!TryGetFashionVariant(variants, variationCode, out variant))
            {
                return new FashionProductViewModel
                {
                    Product = currentContent,
                    Images = currentContent.GetAssets<IContentImage>(_contentLoader, _urlResolver)
                };
            }

            variationCode = variant.Code;
            var defaultPrice = _pricingService.GetDefaultPrice(variationCode);
            var discountedPrice = defaultPrice != null ? _pricingService.GetDiscountPrice(variationCode).UnitPrice : (Money?)null;

            return new FashionProductViewModel
            {
                Product = currentContent,
                Variant = variant,
                ListingPrice = defaultPrice?.UnitPrice ?? _pricingService.GetMoney(0),
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
        }

        public virtual FashionPackageViewModel Create(FashionPackage currentContent)
        {
            var defaultPrice = _pricingService.GetDefaultPrice(currentContent.Code);
            var discountedPrice = defaultPrice != null
                ? _pricingService.GetDiscountPrice(defaultPrice.CatalogKey.CatalogEntryCode).UnitPrice
                : (Money?)null;

            return new FashionPackageViewModel
            {
                Package = currentContent,
                ListingPrice = defaultPrice?.UnitPrice ?? _pricingService.GetMoney(0),
                DiscountedPrice = discountedPrice,
                Images = currentContent.GetAssets<IContentImage>(_contentLoader, _urlResolver),
                IsAvailable = defaultPrice != null,
                Entries = _catalogContentService.GetVariants<FashionVariant>(currentContent).ToList()
            };
        }

        public virtual FashionBundleViewModel Create(FashionBundle currentContent)
        {
            return new FashionBundleViewModel
            {
                Bundle = currentContent,
                Images = currentContent.GetAssets<IContentImage>(_contentLoader, _urlResolver),
                Entries = _catalogContentService.GetVariants<FashionVariant>(currentContent).ToList()
            };
        }

        public virtual FashionVariant SelectVariant(FashionProduct currentContent, string color, string size)
        {
            var variants = _catalogContentService.GetVariants<FashionVariant>(currentContent).ToList();

            FashionVariant variant;
            if (TryGetFashionVariantByColorAndSize(variants, color, size, out variant)
                || TryGetFashionVariantByColorAndSize(variants, color, string.Empty, out variant))//if we cannot find variation with exactly both color and size then we will try to get variant by color only
            {
                return variant;
            }

            return null;
        }

        private static bool TryGetFashionVariant(IEnumerable<FashionVariant> variations, string variationCode, out FashionVariant variation)
        {
            variation = !string.IsNullOrEmpty(variationCode) ?
                variations.FirstOrDefault(x => x.Code == variationCode) :
                variations.FirstOrDefault();

            return variation != null;
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