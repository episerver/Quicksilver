using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Core;
using EPiServer.Filters;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Extensions
{
    public static class ContentExtensions
    {
#pragma warning disable 649
        private static Injected<UrlResolver> _urlResolver;
        private static Injected<ILinksRepository> _linksRepository;
        private static Injected<IContentLoader> _contentLoader;
#pragma warning restore 649

        public static IEnumerable<PageData> GetSiblings(this PageData pageData)
        {
            var filter = new FilterContentForVisitor();
            return  _contentLoader.Service.GetChildren<PageData>(pageData.ParentLink).Where(page => !filter.ShouldFilter(page));
        }

        public static string GetUrl(this VariationContent variant)
        {
            var productLink = variant.GetParentProducts(_linksRepository.Service).FirstOrDefault();
            if (productLink == null)
            {
                return String.Empty;
            }
            var urlBuilder = new UrlBuilder(_urlResolver.Service.GetUrl(productLink));
            urlBuilder.QueryCollection.Add("variationId", variant.Code);
            return urlBuilder.ToString();
        }
    }
}