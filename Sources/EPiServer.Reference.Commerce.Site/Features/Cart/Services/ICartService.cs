using System.Collections.Generic;
using EPiServer.Reference.Commerce.Site.Features.Cart.Models;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.Services
{
    public interface ICartService
    {
        decimal GetLineItemsTotalQuantity();
        CartItem[] GetCartItems();
        Money GetSubTotal();
        Money GetTotal();
        Money GetShippingSubTotal();
        Money GetShippingTotal();
        Money GetTaxTotal();
        Money GetShippingTaxTotal();
        Money GetTotalDiscount();
        Money GetOrderDiscountTotal();
        Money GetShippingDiscountTotal();
        Money ConvertToMoney(decimal amount);
        IEnumerable<OrderForm> GetOrderForms();
        IEnumerable<Shipment> GetShipments();
        bool AddToCart(string code, out string warningMessage);
        void ChangeQuantity(string code, decimal quantity);
        void RemoveLineItem(string code);
        void RunWorkflow(string workFlowName);
        void RunWorkflow(string workFlowName, Dictionary<string, object> context);
        void SaveCart();
        void DeleteCart();
        void InitializeAsWishList();
        void UpdateLineItemSku(string oldCode, string newCode, decimal quantity);
        void SetCartCurrency(Currency currency);
        void ResetLineItemAddresses();
        void RecreateLineItemsBasedOnAddresses(IEnumerable<CartItem> cartItems);
    }
}