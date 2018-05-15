using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Product.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.ViewModelFactories
{
    [ServiceConfiguration(typeof(CartItemViewModelFactory), Lifecycle = ServiceInstanceScope.Singleton)]
    public class CartItemViewModelFactory
    {
        private readonly IContentLoader _contentLoader;
        private readonly IPricingService _pricingService;
        private readonly UrlResolver _urlResolver;
        private readonly CatalogContentService _catalogContentService;
        private readonly IRelationRepository _relationRepository;
        readonly ICartService _cartService;

        public CartItemViewModelFactory(
            IContentLoader contentLoader,
            IPricingService pricingService,
            UrlResolver urlResolver,
            CatalogContentService catalogContentService,
            IRelationRepository relationRepository,
            ICartService cartService)
        {
            _contentLoader = contentLoader;
            _pricingService = pricingService;
            _urlResolver = urlResolver;
            _catalogContentService = catalogContentService;
            _relationRepository = relationRepository;
            _cartService = cartService;
        }

        public virtual CartItemViewModel CreateCartItemViewModel(ICart cart, ILineItem lineItem, EntryContentBase entry)
        {
            var viewModel = new CartItemViewModel
            {
                Code = lineItem.Code,
                DisplayName = entry.DisplayName,
                ImageUrl = entry.GetAssets<IContentImage>(_contentLoader, _urlResolver).FirstOrDefault() ?? "",
                DiscountedPrice = GetDiscountedPrice(cart, lineItem),
                PlacedPrice = _pricingService.GetMoney(lineItem.PlacedPrice),
                Quantity = lineItem.Quantity,
                Url = entry.GetUrl(_relationRepository, _urlResolver),
                Entry = entry,
                IsAvailable = _pricingService.GetPrice(entry.Code) != null,
                DiscountedUnitPrice = GetDiscountedUnitPrice(cart, lineItem),
                IsGift = lineItem.IsGift
            };

            var productLink = entry is VariationContent ?
                entry.GetParentProducts(_relationRepository).FirstOrDefault() :
                entry.ContentLink;

            FashionProduct product;
            if (_contentLoader.TryGet(productLink, out product))
            {
                viewModel.Brand = GetBrand(product);
            }

            var variant = entry as FashionVariant;
            if (variant != null)
            {
                viewModel.AvailableSizes = GetAvailableSizes(product, variant);
            }

            return viewModel;
        }

        private Money? GetDiscountedUnitPrice(ICart cart, ILineItem lineItem)
        {
            var discountedPrice = GetDiscountedPrice(cart, lineItem) / lineItem.Quantity;
            return discountedPrice.GetValueOrDefault().Amount < lineItem.PlacedPrice ? discountedPrice : null;
        }

        private IEnumerable<string> GetAvailableSizes(FashionProduct product, FashionVariant entry)
        {
            return product != null ?
                _catalogContentService.GetVariants<FashionVariant>(product).Where(x => x.Color.Equals(entry.Color, StringComparison.OrdinalIgnoreCase)).Select(x => x.Size)
                : Enumerable.Empty<string>();
        }

        private string GetBrand(FashionProduct product)
        {
            return product?.Brand;
        }

        private Money? GetDiscountedPrice(ICart cart, ILineItem lineItem)
        {
            return cart.Name.Equals(_cartService.DefaultWishListName) ? 
                _pricingService.GetDiscountPrice(lineItem.Code)?.UnitPrice : 
                _pricingService.GetDiscountedPrice(lineItem, cart.Currency);
        }
    }
}