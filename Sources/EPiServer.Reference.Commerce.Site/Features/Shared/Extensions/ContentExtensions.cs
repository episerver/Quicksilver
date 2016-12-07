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
#pragma warning disable 649
        private static Injected<UrlResolver> _urlResolver;
        private static Injected<ILinksRepository> _linksRepository;
        private static Injected<IContentLoader> _contentLoader;
#pragma warning restore 649

        public static IEnumerable<PageData> GetSiblings(this PageData pageData)
        {
            return GetSiblings(pageData, _contentLoader.Service);
        }

        public static IEnumerable<PageData> GetSiblings(this PageData pageData, IContentLoader contentLoader)
        {
            var filter = new FilterContentForVisitor();
            return contentLoader.GetChildren<PageData>(pageData.ParentLink).Where(page => !filter.ShouldFilter(page));
        }

        public static string GetUrl(this VariationContent variant)
        {
            return GetUrl(variant, _linksRepository.Service, _urlResolver.Service);
        }

        public static string GetUrl(this VariationContent variant, ILinksRepository linksRepository, UrlResolver urlResolver)
        {
            var productLink = variant.GetParentProducts(linksRepository).FirstOrDefault();
            if (productLink == null)
            {
                return string.Empty;
            }

            var urlBuilder = new UrlBuilder(urlResolver.GetUrl(productLink));

            if (variant.Code != null)
            {
                urlBuilder.QueryCollection.Add("variationCode", variant.Code);
            }
            
            return urlBuilder.ToString();
        }
    }
}