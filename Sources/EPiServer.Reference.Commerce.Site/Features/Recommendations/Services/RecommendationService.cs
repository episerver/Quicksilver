using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Recommendations.Commerce.Tracking;
using EPiServer.Recommendations.Tracking;
using EPiServer.Recommendations.Tracking.Data;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using System.Collections.Generic;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Recommendations.Services
{
    [ServiceConfiguration(typeof(IRecommendationService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class RecommendationService : IRecommendationService
    {
        private readonly ITrackingService _trackingService;
        private readonly TrackingDataFactory _trackingDataFactory;
        private readonly ReferenceConverter _referenceConverter;
        private readonly IContentLoader _contentLoader;

        public RecommendationService(
            ITrackingService trackingService, 
            TrackingDataFactory trackingDataFactory,
            ReferenceConverter referenceConverter,
            IContentLoader contentLoader)
        {
            _trackingService = trackingService;
            _trackingDataFactory = trackingDataFactory;
            _referenceConverter = referenceConverter;
            _contentLoader = contentLoader;
        }

        public TrackingResponseData SendSearchTracking(HttpContextBase httpContext, string searchTerm, IEnumerable<string> productCodes)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new TrackingResponseData();
            }

            var trackingData = _trackingDataFactory.CreateSearchTrackingData(searchTerm, productCodes, httpContext);
            return _trackingService.Send(trackingData, httpContext);
        }

        public TrackingResponseData SendOrderTracking(HttpContextBase httpContext, IPurchaseOrder order)
        {
            var trackingData = _trackingDataFactory.CreateOrderTrackingData(order, httpContext);
            return _trackingService.Send(trackingData, httpContext);
        }

        public TrackingResponseData SendCategoryTracking(HttpContextBase httpContext, NodeContent category)
        {
            var trackingData = _trackingDataFactory.CreateCategoryTrackingData(category, httpContext);
            return _trackingService.Send(trackingData, httpContext);
        }

        public TrackingResponseData SendFacetTrackingData(HttpContextBase httpContext, string facet)
        {
            var parts = facet.Split(':');
            TrackingDataBase trackingData;

            if (parts[0] == "_node")
            {
                var link = _referenceConverter.GetContentLink(parts[1]);
                var content = _contentLoader.Get<FashionNode>(link);
                trackingData = _trackingDataFactory.CreateCategoryTrackingData(content, httpContext);
            }
            else
            {
                trackingData = _trackingDataFactory.CreateAttributeTrackingData(parts[0], parts[1], httpContext);
            }

            return _trackingService.Send(trackingData, httpContext);
        }

        public TrackingResponseData SendCartTrackingData(HttpContextBase httpContext)
        {
            var trackingData = _trackingDataFactory.CreateCartTrackingData(httpContext);
            return _trackingService.Send(trackingData, httpContext);
        }

        public TrackingResponseData SendWishListTrackingData(HttpContextBase httpContext)
        {
            var trackingData = _trackingDataFactory.CreateWishListTrackingData(httpContext);
            return _trackingService.Send(trackingData, httpContext);
        }
    }
}