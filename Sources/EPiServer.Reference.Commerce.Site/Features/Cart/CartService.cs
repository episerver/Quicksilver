using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Globalization;
using EPiServer.Reference.Commerce.Site.Features.Cart.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Cart.Models;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Website.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Cart
{
    [ServiceConfiguration(typeof(ICartService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class CartService : ICartService
    {
        private readonly Func<CartHelper> _cartHelper;
        private readonly IContentLoader _contentLoader;
        private readonly ReferenceConverter _referenceConverter;
        private readonly CultureInfo _preferredCulture;

        public CartService(Func<CartHelper> cartHelper, IContentLoader contentLoader, ReferenceConverter referenceConverter)
        {
            _cartHelper = cartHelper;
            _contentLoader = contentLoader;
            _referenceConverter = referenceConverter;
            _preferredCulture = ContentLanguage.PreferredCulture;
        }

        public IReadOnlyCollection<LineItem> GetAllLineItems()
        {
            return CartHelper.Cart.GetAllLineItems();
        }

        public IEnumerable<CartItem> GetCartItems()
        {
            CartHelper.RunWorkflow(OrderGroupWorkflowManager.CartValidateWorkflowName);
            var lineItems = CartHelper.Cart.GetAllLineItems();

            // In order to show the images for the items in the cart, we need to load the variants
            var variants = _contentLoader.GetItems(lineItems.Select(x => _referenceConverter.GetContentLink(x.Code)),
                _preferredCulture).OfType<VariationContent>();

            return lineItems.Select(lineItem => new CartItem
            {
                Code = lineItem.Code,
                DisplayName = lineItem.DisplayName,
                ExtendedPrice = lineItem.ToMoney(lineItem.ExtendedPrice + lineItem.OrderLevelDiscountAmount),
                PlacedPrice = lineItem.ToMoney(lineItem.PlacedPrice),
                DiscountPrice = lineItem.ToMoney(Math.Round(lineItem.PlacedPrice - lineItem.Discounts.Cast<LineItemDiscount>().Sum(x => x.DiscountAmount) / lineItem.Quantity, 2)),
                Quantity = lineItem.Quantity,
                Url = lineItem.GetUrl(),
                Variant = variants.FirstOrDefault(variant => variant.Code == lineItem.Code),
                Discounts = lineItem.Discounts.Cast<LineItemDiscount>().Select(x => new OrderDiscountModel 
                {
                    Discount = new Money(x.DiscountAmount, new Currency(CartHelper.Cart.BillingCurrency)),
                    Displayname = x.DisplayMessage
                })
            }).ToList();
        }

        public Money GetTotal()
        {
            CartHelper.RunWorkflow(OrderGroupWorkflowManager.CartValidateWorkflowName);
            var cart = CartHelper.Cart;
            return new Money(cart.GetAllLineItems().Sum(x => x.ExtendedPrice + x.OrderLevelDiscountAmount), cart.BillingCurrency);
        }

        public Money GetTotalDiscount()
        {
            CartHelper.RunWorkflow(OrderGroupWorkflowManager.CartValidateWorkflowName);
            var cart = CartHelper.Cart;
            return new Money(cart.GetAllLineItems().Sum(x => x.LineItemDiscountAmount), cart.BillingCurrency);
        }

        public void AddToCart(string code)
        {
            var entry = CatalogContext.Current.GetCatalogEntry(code);
            CartHelper.AddEntry(entry);
            CartHelper.Cart.ProviderId = "frontend"; // if this is not set explicitly, place price does not get updated by workflow
            AcceptChanges();
        }

        public void ChangeQuantity(string code, int quantity)
        {
            if (quantity == 0)
            {
                RemoveLineItem(code);
            }
            var lineItem = CartHelper.Cart.GetLineItem(code);
            if (lineItem != null)
            {
                lineItem.Quantity = quantity;
                AcceptChanges();
            }
        }

        public void RemoveLineItem(string code)
        {
            var lineItem = CartHelper.Cart.GetLineItem(code);
            if (lineItem != null)
            {
                lineItem.Delete();
                AcceptChanges();
            }
        }

        private void AcceptChanges()
        {
            CartHelper.Cart.OrderForms.First().LineItems.AcceptChanges();
            CartHelper.RunWorkflow(OrderGroupWorkflowManager.CartValidateWorkflowName);
            CartHelper.Cart.AcceptChanges();
        }

        private CartHelper CartHelper
        {
            get { return _cartHelper(); }
        }
    }
}