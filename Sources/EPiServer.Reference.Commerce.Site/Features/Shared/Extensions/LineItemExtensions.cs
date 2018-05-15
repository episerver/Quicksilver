using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using System;
using System.Web;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Extensions
{
    public static class LineItemExtensions
    {
#pragma warning disable 649
        private static Injected<ReferenceConverter> _referenceConverter;
        private static Injected<IContentLoader> _contentLoader;
        private static Injected<ThumbnailUrlResolver> _thumbnailUrlResolver;
#pragma warning restore 649

        public static string GetUrl(this ILineItem lineItem)
        {
            return GetEntryContent(lineItem.Code)?.GetUrl();
        }

        public static string GetFullUrl(this ILineItem lineItem)
        {
            var rightUrl = lineItem.GetUrl();
            var baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
            return new Uri(new Uri(baseUrl), rightUrl).ToString();
        }

        public static string GetThumbnailUrl(this ILineItem lineItem)
        {
            return GetThumbnailUrl(lineItem.Code);
        }

        public static string GetThumbnailUrl(this CartItemViewModel cartItem)
        {
            return GetThumbnailUrl(cartItem.Code);
        }

        private static string GetThumbnailUrl(string code)
        {
            var content = GetEntryContent(code);
            return content == null ? 
                string.Empty : 
                _thumbnailUrlResolver.Service.GetThumbnailUrl(content, "thumbnail");
        }

        private static EntryContentBase GetEntryContent(string code)
        {
            var entryContentLink = _referenceConverter.Service.GetContentLink(code);
            return ContentReference.IsNullOrEmpty(entryContentLink) ? 
                null : 
                _contentLoader.Service.Get<EntryContentBase>(entryContentLink);
        }
    }
}