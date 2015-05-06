using System.Collections.Generic;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Models;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.Models
{
    public class CartItem
    {
        public VariationContent Variant { get; set; }
        public string DisplayName { get; set; }
        public decimal Quantity { get; set; }
        public string Code { get; set; }
        public Money ExtendedPrice { get; set; }
        public Money PlacedPrice { get; set; }
        public Money DiscountPrice { get; set; }
        public string Url { get; set; }
        public IEnumerable<OrderDiscountModel> Discounts { get; set; }
    }
}