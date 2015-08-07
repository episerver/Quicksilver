using System.Collections.Generic;
using EPiServer.Reference.Commerce.Site.Features.Cart.Models;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.Services
{
    public interface ICartService
    {
        decimal GetLineItemsTotalQuantity();
        IEnumerable<CartItem> GetCartItems();
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
        bool AddToCart(string code, out string warningMessage);
        void ChangeQuantity(string code, decimal quantity);
        void RemoveLineItem(string code);
        void RunWorkflow(string workFlowName);
        void SaveCart();
        void DeleteCart();
        void InitializeAsWishList();
        void UpdateLineItemSku(string oldCode, string newCode, decimal quantity);
    }
}