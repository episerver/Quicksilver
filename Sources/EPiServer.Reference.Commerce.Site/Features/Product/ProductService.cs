using System;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.Reference.Commerce.Site.Features.Market;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Core;

namespace EPiServer.Reference.Commerce.Site.Features.Product
{
    [ServiceConfiguration(typeof(IProductService), Lifecycle=ServiceInstanceScope.Singleton)]
    public class ProductService : IProductService
    {
        private readonly IContentLoader _contentLoader;
        private readonly IPricingService _pricingService;
        private readonly UrlResolver _urlResolver;
        private readonly LinksRepository _linksRepository;
        private readonly IRelationRepository _relationRepository;
        private readonly CultureInfo _preferredCulture;
        private readonly ICurrentMarket _currentMarket;
        private readonly CurrencyService _currencyService;
        private readonly Guid _applicationId;

        public ProductService(IContentLoader contentLoader, 
            IPricingService pricingService, 
            UrlResolver urlResolver, 
            LinksRepository linksRepository,
            IRelationRepository relationRepository,
            ICurrentMarket currentMarket,
            CurrencyService currencyService)
        {
            _contentLoader = contentLoader;
            _pricingService = pricingService;
            _urlResolver = urlResolver;
            _linksRepository = linksRepository;
            _relationRepository = relationRepository;
            _preferredCulture = ContentLanguage.PreferredCulture;
            _currentMarket = currentMarket;
            _currencyService = currencyService;
            _applicationId = AppContext.Current.ApplicationId;
        }

        public IEnumerable<ProductViewModel> GetVariationsAndPricesForProducts(IEnumerable<ProductContent> products)
        {
            var variationsToLoad = new Dictionary<ContentReference, ContentReference>();
            var fashionProducts = products.ToList();
            foreach (var product in fashionProducts)
            {
                var relations = _linksRepository.GetRelationsBySource(product.VariantsReference).OfType<ProductVariation>();
                variationsToLoad.Add(relations.First().Target, product.ContentLink);
            }

            var variations = _contentLoader.GetItems(variationsToLoad.Select(x => x.Key), _preferredCulture).Cast<FashionVariant>();

            var productModels = new List<ProductViewModel>();

            foreach (var variation in variations)
            {
                var productContentReference = variationsToLoad.First(x => x.Key == variation.ContentLink).Value;
                var product = fashionProducts.First(x => x.ContentLink == productContentReference);
                productModels.Add(CreateProductViewModel(product, variation));
            }
            return productModels;
        }

        public virtual ProductViewModel GetProductViewModel(ProductContent product)
        {
            var variations = _contentLoader.GetItems(product.GetVariants(), _preferredCulture).
                                            Cast<VariationContent>()
                                           .ToList();

            var variation = variations.FirstOrDefault();
            return CreateProductViewModel(product, variation);
        }

        public virtual ProductViewModel GetProductViewModel(VariationContent variation)
        {
            return CreateProductViewModel(null, variation);
        }

        /// <summary>
        /// Creates the product view model.  
        /// </summary>
        /// <param name="product">The product.</param>
        /// <param name="variation">The variation.</param>
        /// <returns></returns>
        private ProductViewModel CreateProductViewModel(ProductContent product, VariationContent variation)
        {
            if (variation == null)
            {
                return null;
            }
        
            ContentReference productContentReference;
            if (product != null)
            {
                productContentReference = product.ContentLink;
            }
            else
            {
                productContentReference = variation.GetParentProducts(_relationRepository).FirstOrDefault();
                if (ContentReference.IsNullOrEmpty(productContentReference))
                {
                    return null;
                }
            }
            var market = _currentMarket.GetCurrentMarket();
            var currency = _currencyService.GetCurrentCurrency();
            var originalPrice = _pricingService.GetCurrentPrice(variation.Code);
            var discountPrice = GetDiscountPrice(variation, market, currency, originalPrice);
            var image = variation.GetAssets().FirstOrDefault() ?? "";

            return new ProductViewModel
            {
                DisplayName = product != null ? product.DisplayName : variation.DisplayName,
                OriginalPrice = originalPrice,
                Price = discountPrice,
                Image = image,
                Url = variation.GetUrl(),
                ContentLink = productContentReference
            };
        }

        private Money GetDiscountPrice(VariationContent variation, IMarket market, Currency currency, Money orginalPrice)
        {
            var discountPrice = _pricingService.GetDiscountPrice(new CatalogKey(_applicationId, variation.Code), market.MarketId, currency);
            if (discountPrice != null)
            {
                return discountPrice.UnitPrice;
            }

            return orginalPrice;
        }
    }
}