using System.Collections.Generic;
using System.Linq;
using Mediachase.Commerce.Orders;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.Extensions
{
    public static class CartExtensions
    {
        public static IReadOnlyCollection<LineItem> GetAllLineItems(this Mediachase.Commerce.Orders.Cart cart)
        {
            return cart.OrderForms.Any() ? cart.OrderForms.First().LineItems.ToList() : new List<LineItem>();
        }

        public static LineItem GetLineItem(this Mediachase.Commerce.Orders.Cart cart, string code)
        {
            return cart.GetAllLineItems().FirstOrDefault(x => x.Code == code);
        }
    }
}