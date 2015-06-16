using System.Collections.Generic;
using EPiServer.Reference.Commerce.Site.Features.Cart.Models;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

namespace EPiServer.Reference.Commerce.Site.Features.Cart
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
        void AddToCart(string code);
        void ChangeQuantity(string code, int quantity);
        void RemoveLineItem(string code);
        void RunWorkflow(string workFlowName);
        void SaveCart();
    }
}