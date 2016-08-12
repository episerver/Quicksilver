using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using System;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.Models
{
    public class CartItem : IProductModel, ILineItem
    {
        public string DisplayName { get; set; }

        public string ImageUrl { get; set; }

        public string Url { get; set; }

        public string Brand { get; set; }

        public Currency Currency { get; set; }

        public Money? ExtendedPrice { get; set; }

        public decimal PlacedPrice { get; set; }

        public string Code { get; set; }

        public VariationContent Variant { get; set; }

        public decimal Quantity { get; set; }

        public Money? DiscountPrice { get; set; }

        public IEnumerable<OrderDiscountModel> Discounts { get; set; }

        public IEnumerable<string> AvailableSizes { get; set; }

        public bool IsAvailable { get; set; }

        public Guid? AddressId { get; set; }

        public decimal LineItemDiscountAmount { get; set; }

        public decimal OrderLevelDiscountAmount { get; set; }

        public int LineItemId { get; set; }

        decimal ILineItem.FreeQuantity { get; set; }
    }
}