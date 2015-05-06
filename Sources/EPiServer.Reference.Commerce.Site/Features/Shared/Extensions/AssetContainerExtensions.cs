using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Extensions
{
    public static class AssetContainerExtensions
    {
        private static Injected<AssetUrlResolver> _assetUrlResolver;
        private static Injected<UrlResolver> _urlResolver;

        public static string GetDefaultAsset(this IAssetContainer assetContainer)
        {
            return _assetUrlResolver.Service.GetAssetUrl(assetContainer);
        }

        public static IEnumerable<string> GetAssets(this IAssetContainer assetContainer)
        {
            var assets = new List<string>();
            if (assetContainer.CommerceMediaCollection != null)
            {
                assets.AddRange(assetContainer.CommerceMediaCollection.Select(media => _urlResolver.Service.GetUrl(media.AssetLink)));
            }
            if (!assets.Any())
            {
                assets.Add("");
            }
            return assets;
        }

    }
}