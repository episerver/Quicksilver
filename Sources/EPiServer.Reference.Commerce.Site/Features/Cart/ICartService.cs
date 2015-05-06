using System.Collections.Generic;
using EPiServer.Reference.Commerce.Site.Features.Cart.Models;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

namespace EPiServer.Reference.Commerce.Site.Features.Cart
{
    public interface ICartService
    {
        IReadOnlyCollection<LineItem> GetAllLineItems();
        IEnumerable<CartItem> GetCartItems();
        Money GetTotal();
        Money GetTotalDiscount();
        void AddToCart(string code);
        void ChangeQuantity(string code, int quantity);
        void RemoveLineItem(string code);
    }
}