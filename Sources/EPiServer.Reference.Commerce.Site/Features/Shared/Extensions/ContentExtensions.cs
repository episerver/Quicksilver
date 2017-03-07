using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Core;
using EPiServer.Filters;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Extensions
{
    public static class ContentExtensions
    {
        private static Injected<UrlResolver> _urlResolver = default(Injected<UrlResolver>);
        private static Injected<ILinksRepository> _linksRepository = default(Injected<ILinksRepository>);
        private static Injected<IContentLoader> _contentLoader = default(Injected<IContentLoader>);

        public static IEnumerable<PageData> GetSiblings(this PageData pageData)
        {
            return GetSiblings(pageData, _contentLoader.Service);
        }

        public static IEnumerable<PageData> GetSiblings(this PageData pageData, IContentLoader contentLoader)
        {
            var filter = new FilterContentForVisitor();
            return contentLoader.GetChildren<PageData>(pageData.ParentLink).Where(page => !filter.ShouldFilter(page));
        }

        public static string GetUrl(this EntryContentBase entry)
        {
            return GetUrl(entry, _linksRepository.Service, _urlResolver.Service);
        }

        public static string GetUrl(this EntryContentBase entry, ILinksRepository linksRepository, UrlResolver urlResolver)
        {
            var productLink = entry is VariationContent ?
                entry.GetParentProducts(linksRepository).FirstOrDefault() : 
                entry.ContentLink;

            if (productLink == null)
            {
                return string.Empty;
            }

            var urlBuilder = new UrlBuilder(urlResolver.GetUrl(productLink));

            if (entry.Code != null)
            {
                urlBuilder.QueryCollection.Add("variationCode", entry.Code);
            }
            
            return urlBuilder.ToString();
        }
    }
}