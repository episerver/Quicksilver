using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Product.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.ViewModelFactories
{
    [ServiceConfiguration(typeof(CartItemViewModelFactory), Lifecycle = ServiceInstanceScope.Singleton)]
    public class CartItemViewModelFactory 
    {
        private readonly IContentLoader _contentLoader;
        private readonly IPricingService _pricingService;
        private readonly UrlResolver _urlResolver;
        private readonly ICurrentMarket _currentMarket;
        private readonly ICurrencyService _currencyService;
        private readonly IPromotionService _promotionService;
        private readonly AppContextFacade _appContext;
        private readonly ILineItemCalculator _lineItemCalculator;
        private readonly IProductService _productService;
        private readonly IRelationRepository _relationRepository;
        readonly ICartService _cartService;

        public CartItemViewModelFactory(
            IContentLoader contentLoader,
            IPricingService pricingService,
            UrlResolver urlResolver,
            ICurrentMarket currentMarket,
            ICurrencyService currencyService,
            IPromotionService promotionService,
            AppContextFacade appContext,
            ILineItemCalculator lineItemCalculator,
            IProductService productService, 
            IRelationRepository relationRepository, 
            ICartService cartService)
        {
            _contentLoader = contentLoader;
            _pricingService = pricingService;
            _urlResolver = urlResolver;
            _currentMarket = currentMarket;
            _currencyService = currencyService;
            _promotionService = promotionService;
            _appContext = appContext;
            _lineItemCalculator = lineItemCalculator;
            _productService = productService;
            _relationRepository = relationRepository;
            _cartService = cartService;
        }

        public virtual CartItemViewModel CreateCartItemViewModel(ICart cart, ILineItem lineItem, VariationContent variant)
        {
            var productLink = variant.GetParentProducts(_relationRepository).FirstOrDefault();
            var product = _contentLoader.Get<ProductContent>(productLink) as FashionProduct;

            return new CartItemViewModel
            {
                Code = lineItem.Code,
                DisplayName = variant.DisplayName,
                ImageUrl = variant.GetAssets<IContentImage>(_contentLoader, _urlResolver).FirstOrDefault() ?? "",
                DiscountedPrice = GetDiscountedPrice(cart, lineItem),
                PlacedPrice = new Money(lineItem.PlacedPrice, _currencyService.GetCurrentCurrency()),
                Quantity = lineItem.Quantity,
                Url = lineItem.GetUrl(),
                Variant = variant,
                IsAvailable = _pricingService.GetCurrentPrice(variant.Code).HasValue,
                Brand = GetBrand(product),
                AvailableSizes = GetAvailableSizes(product, variant),
                DiscountedUnitPrice = GetDiscountedUnitPrice(cart, lineItem),
                IsGift = lineItem.IsGift
            };
        }

        private Money? GetDiscountedUnitPrice(ICart cart, ILineItem lineItem)
        {
            var discountedPrice = GetDiscountedPrice(cart, lineItem) / lineItem.Quantity;
            return discountedPrice.GetValueOrDefault().Amount < lineItem.PlacedPrice ? discountedPrice : null;
        }

        private IEnumerable<string> GetAvailableSizes(FashionProduct product, VariationContent variant)
        {
            return product != null ?
                _productService.GetVariations(product).Where(x => x.Color.Equals(((FashionVariant)variant).Color, StringComparison.OrdinalIgnoreCase)).Select(x => x.Size)
                : Enumerable.Empty<string>();
        }

        private string GetBrand(FashionProduct product)
        {
            return product != null ? product.Brand : null;
        }

        private Money? GetDiscountedPrice(ICart cart, ILineItem lineItem)
        {
            var marketId = _currentMarket.GetCurrentMarket().MarketId;
            var currency = _currencyService.GetCurrentCurrency();
            if (cart.Name.Equals(_cartService.DefaultWishListName)) 
            {
                var discountedPrice = _promotionService.GetDiscountPrice(new CatalogKey(_appContext.ApplicationId, lineItem.Code), marketId, currency);
                return discountedPrice != null ? discountedPrice.UnitPrice : (Money?)null;
            }
            return lineItem.GetDiscountedPrice(cart.Currency, _lineItemCalculator);
        }
    }
}