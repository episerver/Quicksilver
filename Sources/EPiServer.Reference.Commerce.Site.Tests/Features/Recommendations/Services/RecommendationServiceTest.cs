using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Product.Services;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Services;
using EPiServer.Tracking.Commerce;
using EPiServer.Tracking.Commerce.Data;
using EPiServer.Tracking.Core;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using Moq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Recommendations.Services
{
    public class RecommendationServiceTest
    {
        [Fact]
        public async void TrackCartAsync_ShouldSendCartTrackingData()
        {
            await _subject.TrackCartAsync(_httpContextMock.Object);

            _trackingDataFactoryMock.Verify(m => m.CreateCartTrackingData(_httpContextMock.Object, Enumerable.Empty<CartChangeData>()));

            Assert.NotNull(_trackedData);
            Assert.IsType<CartTrackingData>(_trackedData.Payload);
        }

        [Fact]
        public async void TrackCategoryAsync_ShouldSendCategoryTrackingData()
        {
            var content = new NodeContent();
            await _subject.TrackCategoryAsync(_httpContextMock.Object, content);

            _trackingDataFactoryMock.Verify(m => m.CreateCategoryTrackingData(content, _httpContextMock.Object));

            Assert.NotNull(_trackedData);
            Assert.IsType<CategoryTrackingData>(_trackedData.Payload);
        }

        [Fact]
        public async void TrackCheckoutAsync_ShouldSendCheckoutTrackingData()
        {
            await _subject.TrackCheckoutAsync(_httpContextMock.Object);

            _trackingDataFactoryMock.Verify(m => m.CreateCheckoutTrackingData(_httpContextMock.Object));

            Assert.NotNull(_trackedData);
            Assert.IsType<CheckoutTrackingData>(_trackedData.Payload);
        }

        [Fact]
        public async void TrackProductAsync_ShouldSendProductTrackingData()
        {
            var productCode = "productCode";
            await _subject.TrackProductAsync(_httpContextMock.Object, productCode, false);

            _trackingDataFactoryMock.Verify(m => m.CreateProductTrackingData(productCode, _httpContextMock.Object));

            Assert.NotNull(_trackedData);
            Assert.IsType<ProductTrackingData>(_trackedData.Payload);
        }

        [Fact]
        public async void TrackOrderAsync_ShouldSendOrderTrackingData()
        {
            var purchaseOrderMock = new Mock<IPurchaseOrder>();
            await _subject.TrackOrderAsync(_httpContextMock.Object, purchaseOrderMock.Object);

            _trackingDataFactoryMock.Verify(m => m.CreateOrderTrackingData(purchaseOrderMock.Object, _httpContextMock.Object));

            Assert.NotNull(_trackedData);
            Assert.IsType<OrderTrackingData>(_trackedData.Payload);
        }

        [Fact]
        public async void TrackSearchAsync_ShouldSendSearchTrackingData()
        {
            var searchTerm = "foo";
            var codes = new[] { "productCode" };
            await _subject.TrackSearchAsync(_httpContextMock.Object, searchTerm, codes, 20);

            _trackingDataFactoryMock.Verify(m => m.CreateSearchTrackingData(searchTerm, codes, 20, _httpContextMock.Object));

            Assert.NotNull(_trackedData);
            Assert.IsType<SearchTrackingData>(_trackedData.Payload);
        }

        [Fact]
        public async void TrackWishlistAsync_ShouldWishlistTrackingData()
        {
            await _subject.TrackWishlistAsync(_httpContextMock.Object);

            _trackingDataFactoryMock.Verify(m => m.CreateWishListTrackingData(_httpContextMock.Object));

            Assert.NotNull(_trackedData);
            Assert.IsType<WishListTrackingData>(_trackedData.Payload);
        }

        [Fact]
        public async void TrackProductAsync_ShouldIndcludeMarketIdAttribute()
        {
            var market = new MarketImpl("SomeMarket");
            _currentMarketServiceMock.Setup(x => x.GetCurrentMarket()).Returns(market);

            await _subject.TrackProductAsync(_httpContextMock.Object, null, false);

            Assert.NotNull(_trackedData);
            string marketId;
            _trackedData.Payload.TryGetCustomAttribute("MarketId", out marketId);
            Assert.Equal(market.MarketId.Value, marketId);
        }

        private readonly RecommendationService _subject;
        private readonly Mock<HttpContextBase> _httpContextMock;
        private readonly Mock<ICurrentMarket> _currentMarketServiceMock;
        private readonly Mock<TrackingDataFactory> _trackingDataFactoryMock;
        private TrackingData<CommerceTrackingData> _trackedData;

        public RecommendationServiceTest()
        {
            var httpRequestMock = new Mock<HttpRequestBase>();
            _httpContextMock = new Mock<HttpContextBase>();
            _httpContextMock.Setup(c => c.Request).Returns(httpRequestMock.Object);
            var httpContextItems = new Hashtable();
            _httpContextMock.SetupGet(x => x.Items).Returns(httpContextItems);
            var contentRouteHelperMock = new Mock<IContentRouteHelper>();
            var contextModeResolverMock = new Mock<IContextModeResolver>();
            contextModeResolverMock.SetupGet(m => m.CurrentMode).Returns(ContextMode.Default);
            var productServiceMock = new Mock<IProductService>();

            _currentMarketServiceMock = new Mock<ICurrentMarket>();

            var requestTrackingDataServiceMock = new Mock<IRequestTrackingDataService>();

            _trackingDataFactoryMock = new Mock<TrackingDataFactory>(null, null, null, null, null, null, null, null, null, requestTrackingDataServiceMock.Object);
            _trackingDataFactoryMock
                .Setup(m => m.CreateCartTrackingData(It.IsAny<HttpContextBase>(), It.IsAny<IEnumerable<CartChangeData>>()))
                .Returns<HttpContextBase, IEnumerable<CartChangeData>>(
                    (context, changes) => new CartTrackingData(null, null, null, new RequestData(), null));
            _trackingDataFactoryMock
                .Setup(m => m.CreateCategoryTrackingData(It.IsAny<NodeContent>(), It.IsAny<HttpContextBase>()))
                .Returns<NodeContent, HttpContextBase>(
                    (content, context) => new CategoryTrackingData(null, null, new RequestData(), null));
            _trackingDataFactoryMock
                .Setup(m => m.CreateCheckoutTrackingData(It.IsAny<HttpContextBase>()))
                .Returns<HttpContextBase>(
                    context => new CheckoutTrackingData(null, null, 0, 0, 0, null, new RequestData(), null));
            _trackingDataFactoryMock
                .Setup(m => m.CreateProductTrackingData(It.IsAny<string>(), It.IsAny<HttpContextBase>()))
                .Returns<string, HttpContextBase>(
                    (code, context) => new ProductTrackingData(code, null, new RequestData(), null));
            _trackingDataFactoryMock
                .Setup(m => m.CreateOrderTrackingData(It.IsAny<IPurchaseOrder>(), It.IsAny<HttpContextBase>()))
                .Returns<IPurchaseOrder, HttpContextBase>(
                    (code, context) => new OrderTrackingData(null, null, 0, 0, 0, null, null, new RequestData(), null));
            _trackingDataFactoryMock
                .Setup(m => m.CreateSearchTrackingData(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<int>(), It.IsAny<HttpContextBase>()))
                .Returns<string, IEnumerable<string>, int, HttpContextBase>(
                    (term, codes, count, context) => new SearchTrackingData(term, codes, count, null, new RequestData(), null));
            _trackingDataFactoryMock
                .Setup(m => m.CreateWishListTrackingData(It.IsAny<HttpContextBase>()))
                .Returns<HttpContextBase>(
                    context => new WishListTrackingData(null, null, new RequestData(), null));

            var trackingServiceMock = new Mock<ITrackingService>();
            trackingServiceMock
                .Setup(m => m.Track(It.IsAny<TrackingData<CommerceTrackingData>>(), It.IsAny<HttpContextBase>()))
                .Callback<TrackingData<CommerceTrackingData>, HttpContextBase>(
                    (data, context) => _trackedData = data)
                .Returns(Task.FromResult(0));

            _subject = new RecommendationService(
                () => contentRouteHelperMock.Object,
                contextModeResolverMock.Object,
                productServiceMock.Object,
                _currentMarketServiceMock.Object,
                _trackingDataFactoryMock.Object,
                trackingServiceMock.Object);
        }
    }
}
