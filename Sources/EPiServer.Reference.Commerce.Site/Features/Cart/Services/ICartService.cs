using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using Mediachase.Commerce;
using System.Collections.Generic;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.Services
{
    public interface ICartService
    {
        AddToCartResult AddToCart(ICart cart, string code, decimal quantity);
        void ChangeCartItem(ICart cart, int shipmentId, string code, decimal quantity, string size, string newSize);
        void SetCartCurrency(ICart cart, Currency currency);
        Dictionary<ILineItem, List<ValidationIssue>> ValidateCart(ICart cart);
        Dictionary<ILineItem, List<ValidationIssue>> RequestInventory(ICart cart);
        string DefaultCartName { get; }
        string DefaultWishListName { get; }
        ICart LoadCart(string name);
        ICart LoadOrCreateCart(string name);
        bool AddCouponCode(ICart cart, string couponCode);
        void RemoveCouponCode(ICart cart, string couponCode);
        void RecreateLineItemsBasedOnShipments(ICart cart, IEnumerable<CartItemViewModel> cartItems, IEnumerable<AddressModel> addresses);
        void MergeShipments(ICart cart);
    }
}