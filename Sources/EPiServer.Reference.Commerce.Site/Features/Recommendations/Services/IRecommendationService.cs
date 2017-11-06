using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Tracking.Commerce.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using EPiServer.Personalization.Commerce.Tracking;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.ViewModels;

namespace EPiServer.Reference.Commerce.Site.Features.Recommendations.Services
{
    public interface IRecommendationService
    {
        Task<TrackingResponseData> TrackProduct(HttpContextBase httpContext, string productCode, bool skipRecommendations);
        Task<TrackingResponseData> TrackSearch(HttpContextBase httpContext, string searchTerm, IEnumerable<string> productCodes);
        Task<TrackingResponseData> TrackOrder(HttpContextBase httpContext, IPurchaseOrder order);
        Task<TrackingResponseData> TrackCategory(HttpContextBase httpContext, NodeContent category);
        Task<TrackingResponseData> TrackCart(HttpContextBase httpContext);
        Task<TrackingResponseData> TrackWishlist(HttpContextBase httpContext);
        Task<TrackingResponseData> TrackCheckout(HttpContextBase httpContext);

        IEnumerable<RecommendedProductTileViewModel> GetRecommendedProductTileViewModels(IEnumerable<Recommendation> recommendations);
    }
}