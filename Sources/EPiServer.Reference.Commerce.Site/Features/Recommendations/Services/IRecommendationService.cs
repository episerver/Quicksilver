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
        Task<TrackingResponseData> TrackProductAsync(HttpContextBase httpContext, string productCode, bool skipRecommendations);
        Task<TrackingResponseData> TrackSearchAsync(HttpContextBase httpContext, string searchTerm, IEnumerable<string> productCodes, int totalRecordsCount);
        Task<TrackingResponseData> TrackOrderAsync(HttpContextBase httpContext, IPurchaseOrder order);
        Task<TrackingResponseData> TrackCategoryAsync(HttpContextBase httpContext, NodeContent category);
        Task<TrackingResponseData> TrackCartAsync(HttpContextBase httpContext);
        Task<TrackingResponseData> TrackWishlistAsync(HttpContextBase httpContext);
        Task<TrackingResponseData> TrackCheckoutAsync(HttpContextBase httpContext);

        IEnumerable<RecommendedProductTileViewModel> GetRecommendedProductTileViewModels(IEnumerable<Recommendation> recommendations);
    }
}