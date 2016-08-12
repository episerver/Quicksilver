using EPiServer.Reference.Commerce.Site.Features.Cart.Models;
using Mediachase.Commerce;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.Extensions
{
    public static class CartItemExtensions
    {
        public static CartItem[] ToFlattenedArray(this IEnumerable<CartItem> collection)
        {
            List<CartItem> list = new List<CartItem>();

            foreach (var item in collection)
            {
                for (int i = 0; i < item.Quantity; i++)
                {
                    list.Add(new CartItem
                    {
                        Quantity = 1,
                        AvailableSizes = item.AvailableSizes,
                        Brand = item.Brand,
                        DisplayName = item.DisplayName,
                        Code = item.Code,
                        ImageUrl = item.ImageUrl,
                        IsAvailable = item.IsAvailable,
                        PlacedPrice = item.PlacedPrice,
                        LineItemDiscountAmount = item.LineItemDiscountAmount / item.Quantity,
                        OrderLevelDiscountAmount = item.OrderLevelDiscountAmount / item.Quantity,
                        AddressId = item.AddressId,
                        Url = item.Url,
                        Variant = item.Variant,
                        DiscountPrice = item.DiscountPrice,
                        Discounts = item.Discounts,
                        ExtendedPrice = item.ExtendedPrice,
                        Currency = item.Currency
                    });
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// Aggregates a collection of <see cref="CartIem"/>s by grouping them based on their AddressId and SKU code, then adding
        /// property values from another collection that should come freshly fetch from the current cart.
        /// </summary>
        /// <param name="collection">The <see cref="CartItem"/> collection having the AddressId property set.</param>
        /// <param name="additionalCartItems">The <see cref="CartItem"/> collection having no AddressId but that holds all other property values.</param>
        /// <returns>An array of <see cref="CartItem"/>s grouped by AddressId and Code.</returns>
        public static CartItem[] AggregateCartItems(this IEnumerable<CartItem> collection, IEnumerable<CartItem> additionalCartItems)
        {
            var aggregatedLineItems = (from addressItem in collection
                                       group addressItem by new { addressItem.Code, addressItem.AddressId } into shipmentItem
                                       join cartItem in additionalCartItems on shipmentItem.Key.Code equals cartItem.Code
                                       select new CartItem
                                       {
                                           Code = cartItem.Code,
                                           AddressId = shipmentItem.Key.AddressId,
                                           Quantity = shipmentItem.Sum(x => x.Quantity),
                                           AvailableSizes = cartItem.AvailableSizes,
                                           Brand = cartItem.Brand,
                                           DisplayName = cartItem.DisplayName,
                                           ImageUrl = cartItem.ImageUrl,
                                           PlacedPrice = cartItem.PlacedPrice,
                                           DiscountPrice = cartItem.DiscountPrice,
                                           LineItemDiscountAmount = cartItem.DiscountPrice.HasValue ? 
                                                (cartItem.PlacedPrice - cartItem.DiscountPrice.Value.Amount) * shipmentItem.Sum(x => x.Quantity) : 0,
                                           Discounts = cartItem.Discounts,
                                           IsAvailable = cartItem.IsAvailable,
                                           Url = cartItem.Url,
                                           Variant = cartItem.Variant,
                                           Currency = cartItem.Currency
                                       }).ToArray();

            return aggregatedLineItems;
        }

        /// <summary>
        /// Copies some of the property values from one <see cref="CartItem"/> to another. The properties Quantity, AddressId, ExtendedPrice,
        /// LineItemDiscountAmount and OrderLevelDiscountAmount are excluded and will not be copied.
        /// </summary>
        /// <param name="target">The <see cref="CartItem"/> receiving values from the other item.</param>
        /// <param name="source">The <see cref="CartItem"/> which all values are copied from.</param>
        /// <remarks>This method is used for keeping CartItems used with multi shipment in synch with the
        /// CartItems in the current cart.</remarks>
        public static void SetValues(this CartItem target, CartItem source)
        {
            target.Code = source.Code;
            target.AvailableSizes = source.AvailableSizes;
            target.Brand = source.Brand;
            target.DisplayName = source.DisplayName;
            target.ImageUrl = source.ImageUrl;
            target.PlacedPrice = source.PlacedPrice;
            target.DiscountPrice = source.DiscountPrice;
            target.Discounts = source.Discounts;
            target.IsAvailable = source.IsAvailable;
            target.Url = source.Url;
            target.Variant = source.Variant;
            target.Currency = source.Currency;
        }
    }
}