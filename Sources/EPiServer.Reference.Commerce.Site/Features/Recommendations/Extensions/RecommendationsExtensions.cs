using EPiServer.Personalization.Commerce.Tracking;
using EPiServer.Tracking.Commerce.Data;
using Mediachase.Commerce.Catalog;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Personalization.Commerce.Extensions;

namespace EPiServer.Reference.Commerce.Site.Features.Recommendations.Extensions
{
    public static class RecommendationsExtensions
    {
        private const string ProductAlternatives = "productAlternativesWidget";
        private const string ProductCrossSells = "productCrossSellsWidget";
        private const string Home = "homeWidget";
        private const string Category = "categoryWidget";
        private const string SearchResult = "searchWidget";

        public static IEnumerable<Recommendation> GetHomeRecommendations(this ControllerBase controller)
        {
            return controller.GetRecommendationGroups()
                .Where(x => x.Area == Home)
                .SelectMany(x => x.Recommendations);
        }

        public static IEnumerable<Recommendation> GetCategoryRecommendations(this TrackingResponseData response, ReferenceConverter referenceConverter)
        {
            return response.GetRecommendationGroups(referenceConverter).Where(x => x.Area == Category).SelectMany(x => x.Recommendations);
        }

        public static IEnumerable<Recommendation> GetSearchResultRecommendations(this TrackingResponseData response, ReferenceConverter referenceConverter)
        {
            return response.GetRecommendationGroups(referenceConverter).Where(x => x.Area == SearchResult).SelectMany(x => x.Recommendations);
        }

        public static IEnumerable<Recommendation> GetAlternativeProductsRecommendations(this TrackingResponseData response, ReferenceConverter referenceConverter)
        {
            return response.GetRecommendationGroups(referenceConverter).Where(x => x.Area == ProductAlternatives).SelectMany(x => x.Recommendations);
        }

        public static IEnumerable<Recommendation> GetCrossSellProductsRecommendations(this TrackingResponseData response, ReferenceConverter referenceConverter)
        {
            return response.GetRecommendationGroups(referenceConverter).Where(x => x.Area == ProductCrossSells).SelectMany(x => x.Recommendations);
        }
    }
}