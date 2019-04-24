using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Tracking.Commerce.Data;
using Mediachase.Commerce;
using System;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.Services
{
    public interface ICartService
    {
        AddToCartResult AddToCart(ICart cart, string code, decimal quantity);
        CartChangeData ChangeCartItem(ICart cart, int shipmentId, string code, decimal quantity, string size, string newSize, string displayName);
        void SetCartCurrency(ICart cart, Currency currency);
        IDictionary<ILineItem, IList<ValidationIssue>> RequestInventory(ICart cart);
        IDictionary<ILineItem, IList<ValidationIssue>> ValidateCart(ICart cart);
        string DefaultCartName { get; }
        string DefaultWishListName { get; }
        ICart LoadCart(string name);
        ICart LoadOrCreateCart(string name);
        bool AddCouponCode(ICart cart, string couponCode);
        void RemoveCouponCode(ICart cart, string couponCode);
        void RecreateLineItemsBasedOnShipments(ICart cart, IEnumerable<CartItemViewModel> cartItems, IEnumerable<AddressModel> addresses);
        void MergeShipments(ICart cart);
        void UpdateShippingMethod(ICart cart, int shipmentId, Guid shippingMethodId);
        IEnumerable<RewardDescription> ApplyDiscounts(ICart cart);
    }
}