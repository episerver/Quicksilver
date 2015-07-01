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

        public decimal GetLineItemsTotalQuantity()
        {
            return CartHelper.Cart.GetAllLineItems().Sum(x => x.Quantity);
        }

        public IEnumerable<CartItem> GetCartItems()
        {
            if (CartHelper.IsEmpty)
            {
                return Enumerable.Empty<CartItem>();
            }

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
                DiscountPrice = lineItem.ToMoney(Math.Round(((lineItem.PlacedPrice * lineItem.Quantity) - lineItem.Discounts.Cast<LineItemDiscount>().Sum(x => x.DiscountValue)) / lineItem.Quantity, 2)),
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
            if (CartHelper.IsEmpty)
            {
                return ConvertToMoney(0);
            }

            CartHelper.RunWorkflow(OrderGroupWorkflowManager.CartValidateWorkflowName);

            return ConvertToMoney(CartHelper.Cart.Total);
        }

        public Money GetTotalDiscount()
        {
            decimal amount = 0;

            if (CartHelper.IsEmpty)
            {
                return ConvertToMoney(amount);
            }

            CartHelper.RunWorkflow(OrderGroupWorkflowManager.CartValidateWorkflowName);

            amount = CartHelper.Cart.GetAllLineItems().Sum(x => x.LineItemDiscountAmount);

            return ConvertToMoney(amount);
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
                PurchaseOrderManager.RemoveLineItemFromOrder(CartHelper.Cart, lineItem.LineItemId);
                AcceptChanges();
            }
        }

        private void AcceptChanges()
        {
            if (CartHelper.Cart.OrderForms.Any())
            {
                CartHelper.Cart.OrderForms.First().LineItems.AcceptChanges();
            }
            
            CartHelper.RunWorkflow(OrderGroupWorkflowManager.CartValidateWorkflowName);
            CartHelper.Cart.AcceptChanges();
        }

        private CartHelper CartHelper
        {
            get { return _cartHelper(); }
        }

        /// <summary>
        /// Converts an amount into a Money struct having the same currency as the current cart.
        /// </summary>
        /// <param name="amount">The amount to convert.</param>
        /// <returns>A Money for the provided amount and in the currency of the current cart.</returns>
        public Money ConvertToMoney(decimal amount)
        {
            return new Money(amount, new Currency(CartHelper.Cart.BillingCurrency));
        }

        public Money GetSubTotal()
        {
            decimal amount = CartHelper.Cart.SubTotal + CartHelper.Cart.OrderForms.SelectMany(x => x.Discounts.Cast<OrderFormDiscount>()).Sum(x => x.DiscountAmount);

            return ConvertToMoney(amount);
        }

        public Money GetShippingSubTotal()
        {
            decimal amount = CartHelper.Cart.OrderForms.SelectMany(x => x.Shipments).Sum(x => x.ShipmentTotal) + CartHelper.Cart.OrderForms.SelectMany(x => x.Shipments).Sum(x => x.ShippingDiscountAmount);

            return ConvertToMoney(amount);
        }

        public Money GetShippingTotal()
        {
            return ConvertToMoney(CartHelper.Cart.ShippingTotal);
        }

        public Money GetTaxTotal()
        {
            return ConvertToMoney(CartHelper.Cart.TaxTotal);
        }

        public Money GetShippingTaxTotal()
        {
            decimal amount = CartHelper.Cart.ShippingTotal + CartHelper.Cart.TaxTotal;

            return ConvertToMoney(amount);
        }

        public Money GetOrderDiscountTotal()
        {
            decimal amount = GetOrderForms().SelectMany(x => x.Discounts.Cast<OrderFormDiscount>()).Sum(x => x.DiscountAmount);

            return ConvertToMoney(amount);
        }

        public Money GetShippingDiscountTotal()
        {
            decimal amount = GetOrderForms().SelectMany(x => x.Shipments).SelectMany(x => x.Discounts.Cast<ShipmentDiscount>()).Sum(x => x.DiscountAmount);

            return ConvertToMoney(amount);
        }

        public IEnumerable<OrderForm> GetOrderForms()
        {
            return CartHelper.Cart.OrderForms.Count == 0 ? new[] { new OrderForm() } : CartHelper.Cart.OrderForms.ToArray();
        }

        public void RunWorkflow(string workFlowName)
        {
            CartHelper.RunWorkflow(workFlowName);
        }

        public void SaveCart()
        {
            AcceptChanges();
        }
    }
}