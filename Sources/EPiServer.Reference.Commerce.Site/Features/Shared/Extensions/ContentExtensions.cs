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
        private static Injected<IRelationRepository> _relationRepository = default(Injected<IRelationRepository>);
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

        public static string GetUrl(this EntryContentBase entry, string language)
        {
            return GetUrl(entry, _relationRepository.Service, _urlResolver.Service, language);
        }

        public static string GetUrl(this EntryContentBase entry)
        {
            return GetUrl(entry, _relationRepository.Service, _urlResolver.Service);
        }

        public static string GetUrl(this EntryContentBase entry, IRelationRepository relationRepository, UrlResolver urlResolver) => GetUrl(entry, relationRepository, urlResolver, null);

        public static string GetUrl(this EntryContentBase entry, IRelationRepository relationRepository, UrlResolver urlResolver, string language)
        {
            var productLink = entry is VariationContent ?
                entry.GetParentProducts(relationRepository).FirstOrDefault() :
                entry.ContentLink;

            if (productLink == null)
            {
                return string.Empty;
            }

            var urlBuilder = string.IsNullOrEmpty(language) ? new UrlBuilder(urlResolver.GetUrl(productLink)) : new UrlBuilder(urlResolver.GetUrl(productLink, language));

            if (entry.Code != null)
            {
                urlBuilder.QueryCollection.Add("variationCode", entry.Code);
            }

            return urlBuilder.ToString();
        }
    }
}