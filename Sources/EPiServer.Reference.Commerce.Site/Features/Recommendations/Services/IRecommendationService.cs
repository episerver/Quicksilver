using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Recommendations.Commerce.Tracking;
using EPiServer.Recommendations.Tracking.Data;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.ViewModels;
using System.Collections.Generic;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Recommendations.Services
{
    public interface IRecommendationService
    {
        TrackingResponseData SendSearchTracking(HttpContextBase httpContext, string searchTerm, IEnumerable<string> productCodes);
        TrackingResponseData SendOrderTracking(HttpContextBase httpContext, IPurchaseOrder order);
        TrackingResponseData SendCategoryTracking(HttpContextBase httpContext, NodeContent category);
        TrackingResponseData SendFacetTrackingData(HttpContextBase httpContext, string facet);
        TrackingResponseData SendCartTrackingData(HttpContextBase httpContext);
        TrackingResponseData SendWishListTrackingData(HttpContextBase httpContext);
        TrackingResponseData SendCheckoutTrackingData(HttpContextBase httpContext);
        IEnumerable<RecommendedProductTileViewModel> GetRecommendedProductTileViewModels(IEnumerable<Recommendation> recommendations);
    }
}