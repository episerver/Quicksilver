using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Core;
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
        private readonly LinksRepository _linksRepository;
        private readonly IRelationRepository _relationRepository;
        private readonly CultureInfo _preferredCulture;
        private readonly ICurrentMarket _currentMarket;
        private readonly ICurrencyService _currencyService;
        private readonly AppContextFacade _appContext;
        private readonly ReferenceConverter _referenceConverter;
        private readonly LanguageService _languageService;


        public ProductService(IContentLoader contentLoader,
            IPromotionService promotionService,
            IPricingService pricingService,
            UrlResolver urlResolver,
            LinksRepository linksRepository,
            IRelationRepository relationRepository,
            ICurrentMarket currentMarket,
            ICurrencyService currencyService,
            AppContextFacade appContext,
            ReferenceConverter referenceConverter, 
            LanguageService languageService)
        {
            _contentLoader = contentLoader;
            _promotionService = promotionService;
            _pricingService = pricingService;
            _urlResolver = urlResolver;
            _linksRepository = linksRepository;
            _relationRepository = relationRepository;
            _preferredCulture = ContentLanguage.PreferredCulture;
            _currentMarket = currentMarket;
            _currencyService = currencyService;
            _appContext = appContext;
            _referenceConverter = referenceConverter;
            _languageService = languageService;
        }

        public IEnumerable<FashionVariant> GetVariants(FashionProduct currentContent)
        {
            return _contentLoader
                .GetItems(currentContent.GetVariants(_relationRepository), _preferredCulture)
                .Cast<FashionVariant>()
                .Where(v => v.IsAvailableInCurrentMarket(_currentMarket));
        }

        public string GetSiblingVariantCodeBySize(string siblingCode, string size)
        {
            ContentReference variationReference = _referenceConverter.GetContentLink(siblingCode);
            IEnumerable<Relation> productRelations = _linksRepository.GetRelationsByTarget(variationReference).ToList();
            IEnumerable<ProductVariation> siblingsRelations = _relationRepository.GetRelationsBySource<ProductVariation>(productRelations.First().Source);
            IEnumerable<ContentReference> siblingsReferences = siblingsRelations.Select(x => x.Target);
            IEnumerable<IContent> siblingVariants = _contentLoader.GetItems(siblingsReferences, _preferredCulture);

            var siblingVariant = siblingVariants.OfType<FashionVariant>().First(x => x.Code == siblingCode);

            foreach (var variant in siblingVariants.OfType<FashionVariant>())
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
            if (entry is PackageContent)
            {
                return CreateProductViewModelForEntry((PackageContent)entry);
            }

            if (entry is ProductContent)
            {
                var product = (ProductContent)entry;
                var variant = _contentLoader.GetItems(product.GetVariants(), _preferredCulture).
                                Cast<VariationContent>().FirstOrDefault();

                return CreateProductViewModelForVariant(product, variant);
            }

            if (entry is VariationContent)
            {
                var parentLink = entry.GetParentProducts(_relationRepository).SingleOrDefault();
                var product = _contentLoader.Get<ProductContent>(parentLink);

                return CreateProductViewModelForVariant(product, (VariationContent)entry);
            }

            throw new ArgumentException("BundleContent is not supported", "entry");
        }

        private ProductTileViewModel CreateProductViewModelForEntry(EntryContentBase entry)
        {
            var market = _currentMarket.GetCurrentMarket();
            var currency = _currencyService.GetCurrentCurrency();
            var originalPrice = _pricingService.GetCurrentPrice(entry.Code);
            var discountedPrice = originalPrice.HasValue ? GetDiscountPrice(entry, market, currency, originalPrice.Value) : (Money?)null;
            var image = entry.GetAssets<IContentImage>(_contentLoader, _urlResolver).FirstOrDefault() ?? "";

            return new ProductTileViewModel
            {
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
            var discountedPrice = _promotionService.GetDiscountPrice(new CatalogKey(_appContext.ApplicationId, entry.Code), market.MarketId, currency);
            if (discountedPrice != null)
            {
                return discountedPrice.UnitPrice;
            }

            return originalPrice;
        }
    }
}