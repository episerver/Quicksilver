using System;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.Reference.Commerce.Site.Features.Market;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
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
        private readonly IPromotionService _promotionService;
        private readonly IPricingService _pricingService;
        private readonly UrlResolver _urlResolver;
        private readonly LinksRepository _linksRepository;
        private readonly IRelationRepository _relationRepository;
        private readonly CultureInfo _preferredCulture;
        private readonly ICurrentMarket _currentMarket;
        private readonly ICurrencyService _currencyService;
        private readonly AppContextFacade _appContext;

        public ProductService(IContentLoader contentLoader, 
            IPromotionService promotionService, 
            IPricingService pricingService, 
            UrlResolver urlResolver, 
            LinksRepository linksRepository,
            IRelationRepository relationRepository,
            ICurrentMarket currentMarket,
            ICurrencyService currencyService,
            AppContextFacade appContext)
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
            return new ProductViewModel
            {
                DisplayName = product != null ? product.DisplayName : variation.DisplayName,
                OriginalPrice = _pricingService.GetCurrentPrice(variation.Code),
                Price = _promotionService.GetDiscountPrice(new CatalogKey(_appContext.ApplicationId, variation.Code), market.MarketId, currency).UnitPrice,
                Image = variation.GetAssets<IContentImage>(_contentLoader, _urlResolver).FirstOrDefault() ?? "",
                Url = variation.GetUrl(),
                ContentLink = productContentReference
            };
        }
    }
}