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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Product.Services
{
    [ServiceConfiguration(typeof(IProductService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ProductService : IProductService
    {
        private readonly IContentLoader _contentLoader;
        private readonly IPromotionService _promotionService;
        private readonly IPricingService _pricingService;
        private readonly UrlResolver _urlResolver;
        private readonly IRelationRepository _relationRepository;
        private readonly CultureInfo _preferredCulture;
        private readonly ICurrentMarket _currentMarketService;
        private readonly ICurrencyService _currencyService;
        private readonly ReferenceConverter _referenceConverter;
        private readonly LanguageService _languageService;
        private readonly FilterPublished _filterPublished;        

        public ProductService(IContentLoader contentLoader,
            IPromotionService promotionService,
            IPricingService pricingService,
            UrlResolver urlResolver,
            IRelationRepository relationRepository,
            ICurrentMarket currentMarketService,
            ICurrencyService currencyService,
            ReferenceConverter referenceConverter,
            LanguageService languageService,
            FilterPublished filterPublished)
        {
            _contentLoader = contentLoader;
            _promotionService = promotionService;
            _pricingService = pricingService;
            _urlResolver = urlResolver;
            _relationRepository = relationRepository;
            _preferredCulture = ContentLanguage.PreferredCulture;
            _currentMarketService = currentMarketService;
            _currencyService = currencyService;
            _referenceConverter = referenceConverter;
            _languageService = languageService;
            _filterPublished = filterPublished;
        }

        public IEnumerable<FashionVariant> GetVariants(FashionProduct currentContent)
        {
            return GetAvailableVariants(currentContent.GetVariants(_relationRepository));                
        }

        public string GetSiblingVariantCodeBySize(string siblingCode, string size)
        {
            ContentReference variationReference = _referenceConverter.GetContentLink(siblingCode);
            IEnumerable<ProductVariation> productRelations = _relationRepository.GetParents<ProductVariation>(variationReference);
            IEnumerable<ProductVariation> siblingsRelations = _relationRepository.GetChildren<ProductVariation>(productRelations.First().Parent);
            IEnumerable<ContentReference> siblingsReferences = siblingsRelations.Select(x => x.Child);
            IEnumerable<FashionVariant> siblingVariants = GetAvailableVariants(siblingsReferences);

            var siblingVariant = siblingVariants.First(x => x.Code == siblingCode);

            foreach (var variant in siblingVariants)
            {
                if (variant.Size.Equals(size, StringComparison.OrdinalIgnoreCase) && variant.Code != siblingCode
                    && variant.Color.Equals(siblingVariant.Color, StringComparison.OrdinalIgnoreCase))
                {
                    return variant.Code;
                }
            }

            return null;
        }

        public IEnumerable<ProductTileViewModel> GetProductTileViewModels(IEnumerable<ContentReference> entryLinks)
        {
            var language = _languageService.GetCurrentLanguage();
            var contentItems = _contentLoader.GetItems(entryLinks, language);
            return contentItems.OfType<EntryContentBase>().Select(GetProductTileViewModel);
        }

        public virtual ProductTileViewModel GetProductTileViewModel(EntryContentBase entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            if (entry is PackageContent)
            {
                return CreateProductViewModelForEntry((PackageContent)entry);
            }

            if (entry is ProductContent)
            {
                var product = (ProductContent)entry;
                var variant = GetAvailableVariants(product.GetVariants()).FirstOrDefault();

                return CreateProductViewModelForVariant(product, variant);
            }

            if (entry is VariationContent)
            {
                var parentLink = entry.GetParentProducts(_relationRepository).SingleOrDefault();
                var product = _contentLoader.Get<ProductContent>(parentLink);

                return CreateProductViewModelForVariant(product, (VariationContent)entry);
            }

            throw new ArgumentException("BundleContent is not supported", nameof(entry));
        }

        private IEnumerable<FashionVariant> GetAvailableVariants(IEnumerable<ContentReference> contentLinks)
        {
            return _contentLoader.GetItems(contentLinks, _preferredCulture)
                                                            .OfType<FashionVariant>()
                                                            .Where(v => v.IsAvailableInCurrentMarket(_currentMarketService) && !_filterPublished.ShouldFilter(v));
        }

        private ProductTileViewModel CreateProductViewModelForEntry(EntryContentBase entry)
        {
            var market = _currentMarketService.GetCurrentMarket();
            var currency = _currencyService.GetCurrentCurrency();
            var originalPrice = _pricingService.GetCurrentPrice(entry.Code);
            var discountedPrice = originalPrice.HasValue ? GetDiscountPrice(entry, market, currency, originalPrice.Value) : (Money?)null;
            var image = entry.GetAssets<IContentImage>(_contentLoader, _urlResolver).FirstOrDefault() ?? "";

            return new ProductTileViewModel
            {
                Code = entry.Code,
                DisplayName = entry.DisplayName,
                PlacedPrice = originalPrice.HasValue ? originalPrice.Value : new Money(0, currency),
                DiscountedPrice = discountedPrice,
                ImageUrl = image,
                Url = entry.GetUrl(),
                IsAvailable = originalPrice.HasValue
            };
        }

        private ProductTileViewModel CreateProductViewModelForVariant(ProductContent product, VariationContent variant)
        {
            if (variant == null)
            {
                return null;
            }

            var viewModel = CreateProductViewModelForEntry(variant);
            viewModel.Brand = product is FashionProduct ? ((FashionProduct)product).Brand : string.Empty;

            return viewModel;
        }

        private Money GetDiscountPrice(EntryContentBase entry, IMarket market, Currency currency, Money originalPrice)
        {
            var discountedPrice = _promotionService.GetDiscountPrice(new CatalogKey(entry.Code), market.MarketId, currency);
            if (discountedPrice != null)
            {
                return discountedPrice.UnitPrice;
            }

            return originalPrice;
        }
    }
}