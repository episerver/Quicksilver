using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Recommendations.Commerce.Tracking;
using EPiServer.Recommendations.Tracking;
using EPiServer.Recommendations.Tracking.Data;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Services;
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
        public void SendSearchTracking_WhenSearchQuery_ShouldCallTrackingService()
        {
            _subject.SendSearchTracking(Mock.Of<HttpContextBase>(), "test", Enumerable.Empty<string>());
            _trackingServiceMock.Verify(x => x.Send(It.IsAny<SearchTrackingData>(), It.IsAny<HttpContextBase>()), Times.Once);
        }

        [Fact]
        public void SendSearchTracking_WhenNoSearchQuery_ShouldNotCallTrackingService()
        {
            _subject.SendSearchTracking(Mock.Of<HttpContextBase>(), "", Enumerable.Empty<string>());
            _trackingServiceMock.Verify(x => x.Send(It.IsAny<SearchTrackingData>(), It.IsAny<HttpContextBase>()), Times.Never);
        }

        [Fact]
        public void SendFacetTrackingData_WhenClickedFacetIsCategory_ShouldSendCategoryTrackingData()
        {
            const string nodeCode = "shoes";

            _subject.SendFacetTrackingData(Mock.Of<HttpContextBase>(), string.Format("_node:{0}", nodeCode));

            _referenceConverterMock.Verify(x => x.GetContentLink(It.Is<string>(y => y == nodeCode)), Times.Once);
            _contentLoaderMock.Verify(x => x.Get<FashionNode>(It.IsAny<ContentReference>()), Times.Once);
            _trackingDataFactoryMock.Verify(x => x.CreateCategoryTrackingData(It.IsAny<NodeContent>(), It.IsAny<HttpContextBase>()), Times.Once);
            _trackingServiceMock.Verify(x => x.Send(It.IsAny<TrackingDataBase>(), It.IsAny<HttpContextBase>()), Times.Once);
        }

        [Fact]
        public void SendFacetTrackingData_WhenClickedFacetIsAttribute_ShouldSendAttributeTrackingData()
        {
            const string attributeKey = "color";
            const string attributeValue = "fuchsia";

            _subject.SendFacetTrackingData(Mock.Of<HttpContextBase>(), string.Format("{0}:{1}", attributeKey, attributeValue));

            _trackingDataFactoryMock.Verify(
                x =>
                    x.CreateAttributeTrackingData(It.Is<string>(y => y == attributeKey),
                        It.Is<string>(y => y == attributeValue), It.IsAny<HttpContextBase>()), Times.Once);
            _trackingServiceMock.Verify(x => x.Send(It.IsAny<TrackingDataBase>(), It.IsAny<HttpContextBase>()), Times.Once);
        }

        private readonly RecommendationService _subject;

        private readonly Mock<ITrackingService> _trackingServiceMock;
        private readonly Mock<TrackingDataFactory> _trackingDataFactoryMock;
        private readonly Mock<ReferenceConverter> _referenceConverterMock;
        private readonly Mock<IContentLoader> _contentLoaderMock;

        public RecommendationServiceTests()
        {
            _trackingServiceMock = new Mock<ITrackingService>();
            _trackingDataFactoryMock = new Mock<TrackingDataFactory>(null, null, null, null, null, null, null, null);
            _referenceConverterMock = new Mock<ReferenceConverter>(null, null);
            _contentLoaderMock = new Mock<IContentLoader>();

            _subject = new RecommendationService(
                _trackingServiceMock.Object,
                _trackingDataFactoryMock.Object,
                _referenceConverterMock.Object,
                _contentLoaderMock.Object);
        }
    }
}
