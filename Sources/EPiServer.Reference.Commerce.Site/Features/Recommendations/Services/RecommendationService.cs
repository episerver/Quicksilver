using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Recommendations.Commerce.Tracking;
using EPiServer.Recommendations.Tracking;
using EPiServer.Recommendations.Tracking.Data;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Product.Services;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.ViewModels;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Mediachase.Commerce.Catalog;
using System.Collections.Generic;
using System.Linq;
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
        private readonly LanguageService _languageService;
        private readonly IProductService _productService;
        private readonly IContextModeResolver _contextModeResolver;

        public RecommendationService(
            ITrackingService trackingService, 
            TrackingDataFactory trackingDataFactory,
            ReferenceConverter referenceConverter,
            IContentLoader contentLoader,
            LanguageService languageService,
            IProductService productService,
            IContextModeResolver contextModeResolver)
        {
            _trackingService = trackingService;
            _trackingDataFactory = trackingDataFactory;
            _referenceConverter = referenceConverter;
            _contentLoader = contentLoader;
            _languageService = languageService;
            _productService = productService;
            _contextModeResolver = contextModeResolver;
        }

        public TrackingResponseData SendProductTracking(HttpContextBase httpContext, string productCode, RetrieveRecommendationMode retrieveRecommendationMode)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return new TrackingResponseData();
            }

            var trackingData = _trackingDataFactory.CreateProductTrackingData(productCode, httpContext);
            return _trackingService.Send(trackingData, httpContext, retrieveRecommendationMode);
        }

        public TrackingResponseData SendSearchTracking(HttpContextBase httpContext, string searchTerm, IEnumerable<string> productCodes)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default || string.IsNullOrWhiteSpace(searchTerm))
            {
                return new TrackingResponseData();
            }

            var trackingData = _trackingDataFactory.CreateSearchTrackingData(searchTerm, productCodes, httpContext);
            return _trackingService.Send(trackingData, httpContext, RetrieveRecommendationMode.Enabled);
        }

        public TrackingResponseData SendOrderTracking(HttpContextBase httpContext, IPurchaseOrder order)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return new TrackingResponseData();
            }

            var trackingData = _trackingDataFactory.CreateOrderTrackingData(order, httpContext);
            return _trackingService.Send(trackingData, httpContext, RetrieveRecommendationMode.Enabled);
        }

        public TrackingResponseData SendCategoryTracking(HttpContextBase httpContext, NodeContent category)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return new TrackingResponseData();
            }

            var trackingData = _trackingDataFactory.CreateCategoryTrackingData(category, httpContext);
            return _trackingService.Send(trackingData, httpContext, RetrieveRecommendationMode.Enabled);
        }

        public TrackingResponseData SendCartTrackingData(HttpContextBase httpContext)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return new TrackingResponseData();
            }

            var trackingData = _trackingDataFactory.CreateCartTrackingData(httpContext);
            return _trackingService.Send(trackingData, httpContext, RetrieveRecommendationMode.Disabled);
        }

        public TrackingResponseData SendWishListTrackingData(HttpContextBase httpContext)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return new TrackingResponseData();
            }

            var trackingData = _trackingDataFactory.CreateWishListTrackingData(httpContext);
            return _trackingService.Send(trackingData, httpContext, RetrieveRecommendationMode.Enabled);
        }        

        public TrackingResponseData SendCheckoutTrackingData(HttpContextBase httpContext)
        {
            if (_contextModeResolver.CurrentMode != ContextMode.Default)
            {
                return new TrackingResponseData();
            }

            var trackingData = _trackingDataFactory.CreateCheckoutTrackingData(httpContext);
            return _trackingService.Send(trackingData, httpContext, RetrieveRecommendationMode.Enabled);
        }
        
        public IEnumerable<RecommendedProductTileViewModel> GetRecommendedProductTileViewModels(IEnumerable<Recommendation> recommendations)
        {
            var language = _languageService.GetCurrentLanguage();
            return recommendations.Select(x =>
                new RecommendedProductTileViewModel(
                    x.RecommendationId, 
                    _productService.GetProductTileViewModel(_contentLoader.Get<EntryContentBase>(x.ContentLink, language))
                    )
            );
        }
    }
}