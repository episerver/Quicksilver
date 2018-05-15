using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Marketing.Promotions;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Start.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Features.Start.ViewModels;
using Mediachase.Commerce;
using Moq;
using System.Linq;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Start.Controllers
{
    public class StartControllerTests
    {
        [Fact]
        public void Index_ShouldSetCurrentPageAsStartPage()
        {
            var controller = CreateController();
            var currentPage = new StartPage();

            var result = controller.Index(currentPage);

            Assert.Equal(currentPage, ((StartPageViewModel)result.Model).StartPage);
        }

        [Fact]
        public void Index_WhenThereIsNoCampaign_ShouldHaveZeroPromotions()
        {
            var controller = CreateController();

            var result = controller.Index(new StartPage());

            Assert.Equal(Enumerable.Empty<PromotionViewModel>(), ((StartPageViewModel)result.Model).Promotions);
        }

        [Fact]
        public void Index_WhenCampaignHasTwoPromotions_ShouldCreateModelsForBothPromotions()
        {
            var controller = CreateController();

            var firstPromotion = new SpendAmountGetShippingDiscount() { Name = "The first promotion", IsActive = true };
            var secondPromotion = new SpendAmountGetShippingDiscount() { Name = "The second promotion", IsActive = true };
            var catalogItemSelectionMock = new Mock<CatalogItemSelection>(null, null, null);

            _marketContentLoaderMock.Setup(x => x.GetPromotionItemsForMarket(It.IsAny<IMarket>()))
                .Returns(new PromotionItems[] {
                    new PromotionItems(firstPromotion, catalogItemSelectionMock.Object, catalogItemSelectionMock.Object),
                    new PromotionItems(secondPromotion, catalogItemSelectionMock.Object, catalogItemSelectionMock.Object) });

            var result = controller.Index(new StartPage());

            Assert.Equal(firstPromotion.Name, ((StartPageViewModel)result.Model).Promotions.First().Name);
            Assert.Equal(secondPromotion.Name, ((StartPageViewModel)result.Model).Promotions.Last().Name);
        }

        private Mock<IContentLoader> _contentLoaderMock;
        private Mock<ICurrentMarket> _currentMarketMock;

        private Mock<MarketContentLoader> _marketContentLoaderMock;
        private Mock<PromotionProcessorResolver> _promotionProcessorResolverMock;

        private StartControllerForTest CreateController()
        {
            _contentLoaderMock = new Mock<IContentLoader>();
            _currentMarketMock = new Mock<ICurrentMarket>();

            _promotionProcessorResolverMock = new Mock<PromotionProcessorResolver>(null, null, null);
            _marketContentLoaderMock = new Mock<MarketContentLoader>(_contentLoaderMock.Object, It.IsAny<CampaignInfoExtractor>(), _promotionProcessorResolverMock.Object);

            return new StartControllerForTest(_contentLoaderMock.Object, _currentMarketMock.Object, _marketContentLoaderMock.Object);
        }

        private class StartControllerForTest : StartController
        {
            public StartControllerForTest(
                IContentLoader contentLoader,
                ICurrentMarket currentMarket,
                MarketContentLoader marketContentLoader)
                : base(contentLoader, currentMarket, marketContentLoader)
            { }
        }
    }
}