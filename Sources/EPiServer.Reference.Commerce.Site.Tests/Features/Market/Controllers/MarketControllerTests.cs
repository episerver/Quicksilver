using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Market.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Market.ViewModels;
using EPiServer.Web.Routing;
using FluentAssertions;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using Moq;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Market.Controllers
{
    public class MarketControllerTests
    {
        [Fact]
        public void Set_ShouldReturnResultFromGetUrlOnUrlResolverAsJson()
        {
            const string expectedUrl = "https://github.com/episerver/Quicksilver";

            _marketServiceMock.Setup(x => x.GetMarket(It.IsAny<MarketId>())).Returns(_market);
            _urlResolverMock.Setup(x => x.GetUrl(_contentLink, _language)).Returns(expectedUrl);

            var controller = CreateController();
            var result = controller.Set(_language, _contentLink);

            Assert.Contains(expectedUrl, ((JsonResult)result).Data.ToString());
        }

        [Fact]
        public void Set_ShouldCallSetCurrentMarket()
        {
            _marketServiceMock.Setup(x => x.GetMarket(It.IsAny<MarketId>())).Returns(_market);

            var controller = CreateController();
            controller.Set(_language, _contentLink);
            
            _currentMarketMock.Verify(x => x.SetCurrentMarket(_language), Times.Once);
        }

        [Fact]
        public void Index_WhenCreatingViewModel_ShouldCreateModel()
        {
            var markets = new List<IMarket>
            {
                _currentMarket,
                _market
            };

            var expectedModel = new MarketViewModel()
            {
               ContentLink = _contentLink,
               MarketId = _currentMarket.MarketId.Value,
               Markets = markets.Select(x => new SelectListItem
               {
                   Selected = false,
                   Text = x.MarketName,
                   Value = x.MarketId.Value
               })
            };

            _currentMarketMock.Setup(x => x.GetCurrentMarket()).Returns(_currentMarket);
            _marketServiceMock.Setup(x => x.GetAllMarkets()).Returns(markets);

            var subject = CreateController();
            var result = ((PartialViewResult)subject.Index(_contentLink)).Model as MarketViewModel;
            result.Should().BeEquivalentTo(expectedModel);
        }

        [Fact]
        public void Index_ShouldReturnModelWithEnabledMarkets()
        {
            var disabledMarket = CreateMarketImpl("myMarket", "myMarket", false);

            IEnumerable<IMarket> markets = new MarketImpl[] { _currentMarket, disabledMarket };
            _marketServiceMock.Setup(x => x.GetAllMarkets()).Returns(markets);
            _currentMarketMock.Setup(x => x.GetCurrentMarket()).Returns(_currentMarket);

            var subject = CreateController();
            var result = subject.Index(_contentLink);
            var model = ((ViewResultBase)result).Model as MarketViewModel;
            model.Markets.Should().BeEquivalentTo(new[] { _currentMarket }.Select(x => new SelectListItem { Text = x.MarketName, Value = x.MarketId.Value}));
        }

        [Fact]
        public void Index_ShouldReturnModelWithSortedMarkets()
        {
            IEnumerable<IMarket> markets = new MarketImpl[] { _currentMarket, _market };
            _marketServiceMock.Setup(x => x.GetAllMarkets()).Returns(markets);
            _currentMarketMock.Setup(x => x.GetCurrentMarket()).Returns(_currentMarket);

            var subject = CreateController();
            var result = subject.Index(_contentLink);
            var model = ((ViewResultBase)result).Model as MarketViewModel;
            Assert.Equal( _currentMarket.MarketId.Value, model.Markets.ToList().First().Value);
        }
        
        [Fact]
        public void Index_ShouldReturnPartialViewResultType()
        {
            IEnumerable<IMarket> markets = new MarketImpl[] { _currentMarket, _market };
            _marketServiceMock.Setup(x => x.GetAllMarkets()).Returns(markets);
            _currentMarketMock.Setup(x => x.GetCurrentMarket()).Returns(_currentMarket);

            var contentLink = new ContentReference(11);
            var subject = CreateController();
            var result = subject.Index(contentLink);
            Assert.IsType<PartialViewResult>(result);
        }

        [Fact]
        public void Index_ShouldReturnModelType()
        {
            IEnumerable<IMarket> markets = new MarketImpl[] { _currentMarket, _market };
            _marketServiceMock.Setup(x => x.GetAllMarkets()).Returns(markets);
            _currentMarketMock.Setup(x => x.GetCurrentMarket()).Returns(_currentMarket);

            var contentLink = new ContentReference(11);
            var subject = CreateController();
            var result = subject.Index(contentLink);
            Assert.IsType<MarketViewModel>((result as ViewResultBase).Model);
        }

        private Mock<ICurrentMarket> _currentMarketMock;
        private Mock<IMarketService> _marketServiceMock;
        private Mock<UrlResolver> _urlResolverMock;
        private Mock<LanguageService> _languageServiceMock;
        private Mock<ICurrencyService> _currencyServiceMock;
        private Mock<ICartService> _cartServiceMock;
        private MarketImpl _currentMarket;
        private MarketImpl _market;
        private string _language;
        private ContentReference _contentLink;
        private Mock<CookieService> _cookieServiceMock;
        private Mock<IUpdateCurrentLanguage> _updateCurrentLanguageMock;

        public MarketControllerTests()
        {
            _contentLink = new ContentReference(11);
            _language = "en";
            _currentMarket = CreateMarketImpl( "currentMarket", "currentMarket", true);
            _market = CreateMarketImpl("myMarket", "myMarket", true);
                        
            _currentMarketMock = new Mock<ICurrentMarket>();
            _marketServiceMock = new Mock<IMarketService>();
            _urlResolverMock = new Mock<UrlResolver>();
            _currencyServiceMock = new Mock<ICurrencyService>();
            _cartServiceMock = new Mock<ICartService>();

            _updateCurrentLanguageMock = new Mock<IUpdateCurrentLanguage>();
            _cookieServiceMock = new Mock<CookieService>();

            _languageServiceMock = new Mock<LanguageService>(null, _cookieServiceMock.Object, _updateCurrentLanguageMock.Object);
        }

        private MarketController CreateController()
        {
            return new MarketController(_marketServiceMock.Object, _currentMarketMock.Object, _urlResolverMock.Object, _languageServiceMock.Object, _cartServiceMock.Object, _currencyServiceMock.Object);
        }
        
        private MarketImpl CreateMarketImpl(MarketId marketId,string marketName, bool isEnabled )
        {
            return new MarketImpl(marketId)
                    {
                        DefaultLanguage = CultureInfo.GetCultureInfo(_language),
                        IsEnabled = isEnabled,
                        MarketName = marketName
                    };
        }
   }
}
