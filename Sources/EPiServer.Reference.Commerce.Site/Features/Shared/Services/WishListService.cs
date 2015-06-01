using System;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Globalization;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.WishList.Models;
using EPiServer.Reference.Commerce.Site.Features.WishList.Pages;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Pricing;
using Mediachase.Commerce.Security;
using Mediachase.Commerce.Website.Helpers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Services
{
    [ServiceConfiguration(typeof(IWishListService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class WishListService : IWishListService
    {
        private readonly IContentLoader _contentLoader;
        private readonly ReferenceConverter _referenceConverter;
        private readonly IPricingService _pricingService;
        private readonly IMarketService _marketService;
        private readonly CultureInfo _preferredCulture;

        public WishListService(IContentLoader contentLoader,
                                  ReferenceConverter referenceConverter,
                                  IPricingService pricingService,
                                  IMarketService marketService)
        {
            _contentLoader = contentLoader;
            _referenceConverter = referenceConverter;
            _pricingService = pricingService;
            _marketService = marketService;
            _preferredCulture = ContentLanguage.PreferredCulture;
        }

        public WishListViewModel GetViewModel(WishListPage currentPage)
        {
            var wishListCart = OrderContext.Current.GetCart(CartHelper.WishListName, PrincipalInfo.CurrentPrincipal.GetContactId());
            var products = new List<ProductViewModel>();
            if (!wishListCart.OrderForms.Any())
            {
                return new WishListViewModel
                {
                    CurrentPage = currentPage,
                    Products = products
                };
            }
            var lineItems = wishListCart.OrderForms.First().LineItems;
            var variations = _contentLoader.GetItems(lineItems.Select(x => _referenceConverter.GetContentLink(x.Code)).ToList(),
                _preferredCulture).OfType<VariationContent>().ToList();
            foreach (LineItem lineItem in lineItems)
            {
                var variation = variations.FirstOrDefault(x => x.Code == lineItem.Code);
                if (variation == null)
                {
                    continue;
                }
                var market = _marketService.GetMarket(wishListCart.MarketId);
                var prices = _pricingService.GetPriceList(variation.Code, market.MarketId,
                    new PriceFilter
                    {
                        Currencies = new[] { new Currency(wishListCart.BillingCurrency) }
                    });

                products.Add(new ProductViewModel
                {
                    DisplayName = lineItem.DisplayName,
                    Image = variation.GetDefaultAsset(),
                    Price = prices.Any() ? prices.First().UnitPrice : new Money(0, new Currency(wishListCart.BillingCurrency)),
                    Url = variation.GetUrl(),
                    Code = variation.Code,
                    IsWishList = true
                });
            }
            return new WishListViewModel
            {
                CurrentPage = currentPage,
                Products = products
            };
        }

        public bool AddItem(string code, out string warningMessage)
        {
            var wishListCart = OrderContext.Current.GetCart(CartHelper.WishListName, PrincipalInfo.CurrentPrincipal.GetContactId());
            var entry = CatalogContext.Current.GetCatalogEntry(code);
            var cartHelper = new CartHelper(wishListCart);
            cartHelper.AddEntry(entry);

            cartHelper.Cart.ProviderId = "frontend"; // if this is not set explicitly, place price does not get updated by workflow
            var workflowResult = OrderGroupWorkflowManager.RunWorkflow(cartHelper.Cart, OrderGroupWorkflowManager.CartValidateWorkflowName);
            var warnings = OrderGroupWorkflowManager.GetWarningsFromWorkflowResult(workflowResult).ToArray();

            warningMessage = warnings.Any() ? String.Join(" ", warnings) : null;
            return cartHelper.LineItems.Select(x => x.Code).Contains(code);
        }

        public void RemoveItem(string code)
        {
            var wishListCart = OrderContext.Current.GetCart(CartHelper.WishListName, PrincipalInfo.CurrentPrincipal.GetContactId());
            var cartHelper = new CartHelper(wishListCart);
            var orderForm = wishListCart.OrderForms.Any() ? wishListCart.OrderForms.First() : wishListCart.OrderForms.AddNew();
            var lineItem = orderForm.LineItems.FirstOrDefault(x => x.Code == code);
            if (lineItem != null)
            {
                lineItem.Delete();
                orderForm.LineItems.AcceptChanges();
                cartHelper.RunWorkflow(OrderGroupWorkflowManager.CartValidateWorkflowName);
                cartHelper.Cart.AcceptChanges();
            }
        }

        public void Delete()
        {
            var wishListCart = OrderContext.Current.GetCart(CartHelper.WishListName, PrincipalInfo.CurrentPrincipal.GetContactId());
            var cartHelper = new CartHelper(wishListCart);
            var orderForm = wishListCart.OrderForms.Any() ? wishListCart.OrderForms.First() : wishListCart.OrderForms.AddNew();
            foreach (LineItem lineItem in orderForm.LineItems)
            {
                lineItem.Delete();
            }
            orderForm.LineItems.AcceptChanges();
            cartHelper.RunWorkflow(OrderGroupWorkflowManager.CartValidateWorkflowName);
            cartHelper.Cart.AcceptChanges();
        }
    }
}