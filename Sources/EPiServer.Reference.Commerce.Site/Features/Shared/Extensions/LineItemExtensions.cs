using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Reference.Commerce.Site.Features.Cart.Models;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Orders;
using System;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Extensions
{
    public static class LineItemExtensions
    {
#pragma warning disable 649
        private static Injected<ReferenceConverter> _referenceConverter;
        private static Injected<IContentLoader> _contentLoader;
        private static Injected<ThumbnailUrlResolver> _thumbnailUrlResolver;
#pragma warning restore 649

        public static string GetUrl(this LineItem lineItem)
        {
            var variantLink = _referenceConverter.Service.GetContentLink(lineItem.Code);
            var variant = _contentLoader.Service.Get<VariationContent>(variantLink);
            return variant.GetUrl();
        }

        public static string GetFullUrl(this LineItem lineItem)
        {
            var rightUrl = lineItem.GetUrl();
            var baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
            return new Uri(new Uri(baseUrl), rightUrl).ToString();
        }

        public static string GetThumbnailUrl(this LineItem lineItem)
        {
            return GetThumbnailUrl(lineItem.Code);
        }

        public static string GetThumbnailUrl(this CartItem cartItem)
        {
            return GetThumbnailUrl(cartItem.Code);
        }

        private static string GetThumbnailUrl(string code)
        {
            var content = _contentLoader.Service.Get<VariationContent>(_referenceConverter.Service.GetContentLink(code, CatalogContentType.CatalogEntry));

            return _thumbnailUrlResolver.Service.GetThumbnailUrl(content, "thumbnail");
        }    

        public static Money PlacedPriceTotal(this LineItem lineItem)
        {
            return lineItem.ToMoney(lineItem.PlacedPrice * lineItem.Quantity);
        }

        /// <summary>
        /// Gets the amount of a line item with all line item discounts subtracted.
        /// </summary>
        /// <param name="lineItem">The line item holding the amount.</param>
        /// <returns>the extended price with any order level amounts excluded.</returns>
        /// <remarks>If an order contains order level discounts then that amount will be divided on the line items and included in the extended price.
        /// This is not always desired. This method shows the extended price for a line item including 
        /// line item discounts, but without involving any order level discounts.
        /// </remarks>
        public static Money ExtendedPriceTotal(this LineItem lineItem)
        {
            return lineItem.ToMoney(lineItem.ExtendedPrice + lineItem.OrderLevelDiscountAmount);
        }

        public static Money ToMoney(this LineItem lineItem, decimal amount)
        {
            return lineItem.Parent.Parent.ToMoney(amount);
        }
    }
}