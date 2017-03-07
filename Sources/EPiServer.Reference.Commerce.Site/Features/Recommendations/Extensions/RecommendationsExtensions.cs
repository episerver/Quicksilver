using EPiServer.Core;
using EPiServer.Recommendations.Commerce.Tracking;
using EPiServer.Recommendations.Tracking.Data;
using Mediachase.Commerce.Catalog;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Recommendations.Extensions
{
    public static class RecommendationsExtensions
    {
        private const string ProductAlternatives = "productAlternativesWidget";
        private const string ProductCrossSells = "productCrossSellsWidget";
        private const string Home = "homeWidget";
        private const string Category = "categoryWidget";
        private const string SearchResult = "searchWidget";

        public static IEnumerable<ContentReference> GetAlternativeProductsRecommendations(this ControllerBase controller)
        {
            return controller.GetRecommendations()
                .Where(x => x.Area == ProductAlternatives)
                .SelectMany(x => x.RecommendedItems)
                .Take(3);
        }

        public static IEnumerable<ContentReference> GetCrossSellProductsRecommendations(this ControllerBase controller)
        {
            return controller.GetRecommendations()
                .Where(x => x.Area == ProductCrossSells)
                .SelectMany(x => x.RecommendedItems);
        }

        public static IEnumerable<ContentReference> GetHomeRecommendations(this ControllerBase controller)
        {
            return controller.GetRecommendations()
                .Where(x => x.Area == Home)
                .SelectMany(x => x.RecommendedItems);
        }

        public static IEnumerable<ContentReference> GetCategoryRecommendations(this TrackingResponseData response, ReferenceConverter referenceConverter)
        {
            return response.SmartRecs != null ? 
                response.SmartRecs.Where(x => x.Position == Category).SelectMany(x => x.Recs)
                .Select(x => referenceConverter.GetContentLink(x.RefCode)) 
                : null;
        }

        public static IEnumerable<ContentReference> GetSearchResultRecommendations(this TrackingResponseData response, ReferenceConverter referenceConverter)
        {
            return response.SmartRecs != null ? 
                response.SmartRecs.Where(x => x.Position == SearchResult).SelectMany(x => x.Recs)
                .Select(x => referenceConverter.GetContentLink(x.RefCode)) 
                : null;
        }
    }
}