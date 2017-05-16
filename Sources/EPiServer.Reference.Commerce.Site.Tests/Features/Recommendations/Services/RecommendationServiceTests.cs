using EPiServer.Recommendations.Commerce.Tracking;
using EPiServer.Recommendations.Tracking;
using EPiServer.Recommendations.Tracking.Data;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Product.Services;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Services;
using EPiServer.Web;
using Mediachase.Commerce.Catalog;
using Moq;
using System.Linq;
using System.Web;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Recommendations.Services
{
    public class RecommendationServiceTests
    {
        [Fact]
        public void SendProductTracking_WhenInViewMode_ShouldCallTrackingService()
        {
            _subject.SendProductTracking(Mock.Of<HttpContextBase>(), "productcode", RetrieveRecommendationMode.Enabled);
            _trackingServiceMock.Verify(x => x.Send(It.IsAny<ProductTrackingData>(), It.IsAny<HttpContextBase>(), RetrieveRecommendationMode.Enabled), Times.Once);
        }

        [Fact]
        public void SendProductTracking_WhenInEditMode_ShouldNotCallTrackingService()
        {
            _contextModeResolverMock.SetupGet(x => x.CurrentMode).Returns(ContextMode.Edit);
            _subject.SendProductTracking(Mock.Of<HttpContextBase>(), "productcode", RetrieveRecommendationMode.Enabled);
            _trackingServiceMock.Verify(x => x.Send(It.IsAny<ProductTrackingData>(), It.IsAny<HttpContextBase>(), RetrieveRecommendationMode.Enabled), Times.Never);
        }

        [Fact]
        public void SendSearchTracking_WhenSearchQueryInViewMode_ShouldCallTrackingService()
        {
            _subject.SendSearchTracking(Mock.Of<HttpContextBase>(), "test", Enumerable.Empty<string>());
            _trackingServiceMock.Verify(x => x.Send(It.IsAny<SearchTrackingData>(), It.IsAny<HttpContextBase>(), RetrieveRecommendationMode.Enabled), Times.Once);
        }

        [Fact]
        public void SendSearchTracking_WhenSearchQueryInEditMode_ShouldNotCallTrackingService()
        {
            _contextModeResolverMock.SetupGet(x => x.CurrentMode).Returns(ContextMode.Edit);
            _subject.SendSearchTracking(Mock.Of<HttpContextBase>(), "test", Enumerable.Empty<string>());
            _trackingServiceMock.Verify(x => x.Send(It.IsAny<SearchTrackingData>(), It.IsAny<HttpContextBase>(), RetrieveRecommendationMode.Enabled), Times.Never);
        }

        [Fact]
        public void SendSearchTracking_WhenNoSearchQuery_ShouldNotCallTrackingService()
        {
            _subject.SendSearchTracking(Mock.Of<HttpContextBase>(), "", Enumerable.Empty<string>());
            _trackingServiceMock.Verify(x => x.Send(It.IsAny<SearchTrackingData>(), It.IsAny<HttpContextBase>(), RetrieveRecommendationMode.Enabled), Times.Never);
        }

        [Fact]
        public void SendCheckoutTrackingData_WhenInViewMode_ShouldCallTrackingService()
        {
            _subject.SendCheckoutTrackingData(Mock.Of<HttpContextBase>());
            _trackingServiceMock.Verify(x => x.Send(It.IsAny<CheckoutTrackingData>(), It.IsAny<HttpContextBase>(), RetrieveRecommendationMode.Enabled), Times.Once);
        }

        [Fact]
        public void SendCheckoutTrackingData_WhenInEditMode_ShouldNotCallTrackingService()
        {
            _contextModeResolverMock.SetupGet(x => x.CurrentMode).Returns(ContextMode.Edit);
            _subject.SendCheckoutTrackingData(Mock.Of<HttpContextBase>());
            _trackingServiceMock.Verify(x => x.Send(It.IsAny<CheckoutTrackingData>(), It.IsAny<HttpContextBase>(), RetrieveRecommendationMode.Enabled), Times.Never);
        }

        [Fact]
        public void SendCartTrackingData_WhenInViewMode_ShouldCallTrackingServiceWithoutRetrieveRecommendations()
        {
            _subject.SendCartTrackingData(Mock.Of<HttpContextBase>());
            _trackingServiceMock.Verify(x => x.Send(It.IsAny<CheckoutTrackingData>(), It.IsAny<HttpContextBase>(), RetrieveRecommendationMode.Disabled), Times.Once);
        }

        [Fact]
        public void SendCartTrackingData_WhenInEditMode_ShouldNotCallTrackingService()
        {
            _contextModeResolverMock.SetupGet(x => x.CurrentMode).Returns(ContextMode.Edit);
            _subject.SendCartTrackingData(Mock.Of<HttpContextBase>());
            _trackingServiceMock.Verify(x => x.Send(It.IsAny<CheckoutTrackingData>(), It.IsAny<HttpContextBase>(), RetrieveRecommendationMode.Disabled), Times.Never);
        }

        private readonly RecommendationService _subject;

        private readonly Mock<ITrackingService> _trackingServiceMock;
        private readonly Mock<TrackingDataFactory> _trackingDataFactoryMock;
        private readonly Mock<ReferenceConverter> _referenceConverterMock;
        private readonly Mock<IContentLoader> _contentLoaderMock;
        private readonly Mock<LanguageService> _languageServiceMock;
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<IContextModeResolver> _contextModeResolverMock;

        public RecommendationServiceTests()
        {
            _trackingServiceMock = new Mock<ITrackingService>();
            _trackingDataFactoryMock = new Mock<TrackingDataFactory>(null, null, null, null, null, null, null, null, null, null);
            _referenceConverterMock = new Mock<ReferenceConverter>(null, null);
            _contentLoaderMock = new Mock<IContentLoader>();
            _languageServiceMock = new Mock<LanguageService>(null, null, null);
            _productServiceMock = new Mock<IProductService>();
            _contextModeResolverMock = new Mock<IContextModeResolver>();
            _contextModeResolverMock.SetupGet(x => x.CurrentMode).Returns(ContextMode.Default);

            _subject = new RecommendationService(
                _trackingServiceMock.Object,
                _trackingDataFactoryMock.Object,
                _referenceConverterMock.Object,
                _contentLoaderMock.Object,
                _languageServiceMock.Object,
                _productServiceMock.Object,
                _contextModeResolverMock.Object);
        }
    }
}
