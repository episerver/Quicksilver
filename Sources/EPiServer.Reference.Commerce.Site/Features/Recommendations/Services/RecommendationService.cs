using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Personalization.Commerce.Tracking;
using EPiServer.Reference.Commerce.Site.Features.Product.Services;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.ViewModels;
using EPiServer.ServiceLocation;
using EPiServer.Tracking.Commerce;
using EPiServer.Tracking.Commerce.Data;
using EPiServer.Tracking.Core;
using EPiServer.Web;
using EPiServer.Web.Routing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Recommendations.Services
{
    [ServiceConfiguration(typeof(IRecommendationService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class RecommendationService : IRecommendationService
    {
        private readonly ServiceAccessor<IContentRouteHelper> _contentRouteHelperAccessor;
        private readonly IContextModeResolver _contextModeResolver;
        private readonly IProductService _productService;
        private readonly TrackingDataFactory _trackingDataFactory;
        private readonly ITrackingService _trackingService;

        public RecommendationService(
            ServiceAccessor<IContentRouteHelper> contentRouteHelperAccessor,
            IContextModeResolver contextModeResolver,
            IProductService productService,
            TrackingDataFactory trackingDataFactory,
            ITrackingService trackingService)
        {
            _contentRouteHelperAccessor = contentRouteHelperAccessor;
            _contextModeResolver = contextModeResolver;
            _productService = productService;
            _trackingDataFactory = trackingDataFactory;
            _trackingService = trackingService;
        }

        public async Task<TrackingResponseData> TrackProduct(HttpContextBase httpContext, string productCode, bool skipRecommendations)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return null;
            }

            var trackingData = _trackingDataFactory.CreateProductTrackingData(productCode, httpContext);

            if (skipRecommendations)
            {
                trackingData.SkipRecommendations();
            }
           
            return await _trackingService.TrackAsync(trackingData, httpContext, _contentRouteHelperAccessor().Content);
        }

        public async Task<TrackingResponseData> TrackSearch(HttpContextBase httpContext, string searchTerm, IEnumerable<string> productCodes)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default || string.IsNullOrWhiteSpace(searchTerm))
            {
                return null;
            }

            var trackingData = _trackingDataFactory.CreateSearchTrackingData(searchTerm, productCodes, httpContext);
            return await _trackingService.TrackAsync(trackingData, httpContext, _contentRouteHelperAccessor().Content);
        }

        public async Task<TrackingResponseData> TrackOrder(HttpContextBase httpContext, IPurchaseOrder order)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return null;
            }

            var trackingData = _trackingDataFactory.CreateOrderTrackingData(order, httpContext);
            return await _trackingService.TrackAsync(trackingData, httpContext, _contentRouteHelperAccessor().Content);
        }

        public async Task<TrackingResponseData> TrackCategory(HttpContextBase httpContext, NodeContent category)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return null;
            }

            var trackingData = _trackingDataFactory.CreateCategoryTrackingData(category, httpContext);
            return await _trackingService.TrackAsync(trackingData, httpContext, _contentRouteHelperAccessor().Content);
        }

        public async Task<TrackingResponseData> TrackCart(HttpContextBase httpContext)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return null;
            }

            var trackingData = _trackingDataFactory.CreateCartTrackingData(httpContext);
            trackingData.SkipRecommendations();
            return await _trackingService.TrackAsync(trackingData, httpContext, _contentRouteHelperAccessor().Content);
        }

        public async Task<TrackingResponseData> TrackWishlist(HttpContextBase httpContext)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return null;
            }

            var trackingData = _trackingDataFactory.CreateWishListTrackingData(httpContext);
            return await _trackingService.TrackAsync(trackingData, httpContext, _contentRouteHelperAccessor().Content);
        }

        public async Task<TrackingResponseData> TrackCheckout(HttpContextBase httpContext)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return null;
            }

            var trackingData = _trackingDataFactory.CreateCheckoutTrackingData(httpContext);
            return await _trackingService.TrackAsync(trackingData, httpContext, _contentRouteHelperAccessor().Content);
        }

        public IEnumerable<RecommendedProductTileViewModel> GetRecommendedProductTileViewModels(IEnumerable<Recommendation> recommendations)
        {
            return recommendations.Select(x =>
                new RecommendedProductTileViewModel(
                    x.RecommendationId,
                    _productService.GetProductTileViewModel(x.ContentLink))
            );
    }
    }
}