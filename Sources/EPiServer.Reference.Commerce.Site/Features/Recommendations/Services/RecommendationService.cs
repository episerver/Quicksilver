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
using Mediachase.Commerce;
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
        private readonly ICurrentMarket _currentMarketService;
        private readonly TrackingDataFactory _trackingDataFactory;
        private readonly ITrackingService _trackingService;

        public RecommendationService(
            ServiceAccessor<IContentRouteHelper> contentRouteHelperAccessor,
            IContextModeResolver contextModeResolver,
            IProductService productService,
            ICurrentMarket currentMarketService,
            TrackingDataFactory trackingDataFactory,
            ITrackingService trackingService)
        {
            _currentMarketService = currentMarketService;
            _contentRouteHelperAccessor = contentRouteHelperAccessor;
            _contextModeResolver = contextModeResolver;
            _productService = productService;
            _trackingDataFactory = trackingDataFactory;
            _trackingService = trackingService;
        }

        public async Task<TrackingResponseData> TrackProductAsync(HttpContextBase httpContext, string productCode, bool skipRecommendations)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return null;
            }

            var trackingData = _trackingDataFactory.CreateProductTrackingData(productCode, httpContext);
            AddMarketAttribute(trackingData);

            if (skipRecommendations)
            {
                trackingData.SkipRecommendations();
            }
           
            return await _trackingService.TrackAsync(trackingData, httpContext, _contentRouteHelperAccessor().Content);
        }

        public async Task<TrackingResponseData> TrackSearchAsync(HttpContextBase httpContext, string searchTerm, IEnumerable<string> productCodes, int totalRecordsCount)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default || string.IsNullOrWhiteSpace(searchTerm))
            {
                return null;
            }

            var trackingData = _trackingDataFactory.CreateSearchTrackingData(searchTerm, productCodes, totalRecordsCount, httpContext);
            AddMarketAttribute(trackingData);

            return await _trackingService.TrackAsync(trackingData, httpContext, _contentRouteHelperAccessor().Content);
        }

        public async Task<TrackingResponseData> TrackOrderAsync(HttpContextBase httpContext, IPurchaseOrder order)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return null;
            }

            var trackingData = _trackingDataFactory.CreateOrderTrackingData(order, httpContext);
            AddMarketAttribute(trackingData);

            return await _trackingService.TrackAsync(trackingData, httpContext, _contentRouteHelperAccessor().Content);
        }

        public async Task<TrackingResponseData> TrackCategoryAsync(HttpContextBase httpContext, NodeContent category)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return null;
            }

            var trackingData = _trackingDataFactory.CreateCategoryTrackingData(category, httpContext);
            AddMarketAttribute(trackingData);

            return await _trackingService.TrackAsync(trackingData, httpContext, _contentRouteHelperAccessor().Content);
        }

        public async Task<TrackingResponseData> TrackCartAsync(HttpContextBase httpContext) =>
            await TrackCartAsync(httpContext, Enumerable.Empty<CartChangeData>());

        public async Task<TrackingResponseData> TrackCartAsync(HttpContextBase httpContext, IEnumerable<CartChangeData> cartChanges)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return null;
            }

            var trackingData = _trackingDataFactory.CreateCartTrackingData(httpContext, cartChanges);
            AddMarketAttribute(trackingData);
            trackingData.SkipRecommendations();

            return await _trackingService.TrackAsync(trackingData, httpContext, _contentRouteHelperAccessor().Content);
        }

        public async Task<TrackingResponseData> TrackWishlistAsync(HttpContextBase httpContext)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return null;
            }

            var trackingData = _trackingDataFactory.CreateWishListTrackingData(httpContext);
            AddMarketAttribute(trackingData);

            return await _trackingService.TrackAsync(trackingData, httpContext, _contentRouteHelperAccessor().Content);
        }

        public async Task<TrackingResponseData> TrackCheckoutAsync(HttpContextBase httpContext)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return null;
            }

            var trackingData = _trackingDataFactory.CreateCheckoutTrackingData(httpContext);
            AddMarketAttribute(trackingData);

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

        /// <summary>
        /// Adds the current market ID as a custom tracking attribute, so that it could
        /// be used in the Personalization portal e.g. to filter widgets depending on
        /// the user's market. This is mainly to demonstrate the use of custom attributes,
        /// it will not automatically affect the tracking or recommendations.
        /// </summary>
        /// <param name="trackingData">The tracking data.</param>
        private void AddMarketAttribute(CommerceTrackingData trackingData)
        {
            var currentMarket = _currentMarketService.GetCurrentMarket();
            if (currentMarket == null)
            {
                return;
            }

            trackingData.SetCustomAttribute("MarketId", currentMarket.MarketId.Value);
        }
    }
}