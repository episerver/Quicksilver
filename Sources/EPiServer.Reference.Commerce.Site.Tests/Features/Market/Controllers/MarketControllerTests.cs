using System.Globalization;
using System.Web.Mvc;
using System.Linq;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Market;
using EPiServer.Reference.Commerce.Site.Features.Market.Controllers;
using EPiServer.Web.Routing;
using FluentAssertions;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections;
using System.Collections.Generic;
using EPiServer.Reference.Commerce.Site.Features.Market.Models;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Market.Controllers
{
    [TestClass]
    public class MarketControllerTests
    {
        [TestMethod]
        public void Set_ShouldReturnResultFromGetUrlOnUrlResolverAsJson()
        {
            const string expectedUrl = "https://github.com/episerver/Quicksilver";

            _mockMarketService.Setup(x => x.GetMarket(It.IsAny<MarketId>())).Returns(_market);
            _mockUrlResolver.Setup(x => x.GetUrl(_contentLink, _language)).Returns(expectedUrl);

            var controller = CreateController();
            var result = controller.Set(_language, _contentLink);

            Assert.IsTrue(((JsonResult)result).Data.ToString().Contains(expectedUrl));
        }

        [TestMethod]
        public void Set_ShouldCallSetCurrentLanguageOnLanguageService()
        {
            _mockMarketService.Setup(x => x.GetMarket(It.IsAny<MarketId>())).Returns(_market);

            var controller = CreateController();
            controller.Set(_language, _contentLink);

            _mockLanguageService.Verify(x => x.SetCurrentLanguage(_language), Times.Once);
        }

        [TestMethod]
        public void Set_ShouldCallSetCurrentMarket()
        {
            _mockMarketService.Setup(x => x.GetMarket(It.IsAny<MarketId>())).Returns(_market);

            var controller = CreateController();
            controller.Set(_language, _contentLink);
            
            _mockCurrentMarket.Verify(x => x.SetCurrentMarket(_language), Times.Once);
        }

        [TestMethod]
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
               CurrentMarket = _currentMarket,
               Markets = markets
            };

            _mockCurrentMarket.Setup(x => x.GetCurrentMarket()).Returns(_currentMarket);
            _mockMarketService.Setup(x => x.GetAllMarkets()).Returns(markets);

            var subject = CreateController();
            var result = ((PartialViewResult)subject.Index(_contentLink)).Model as MarketViewModel;
            result.ShouldBeEquivalentTo(expectedModel);
        }

        [TestMethod]
        public void Index_ShouldReturnModelWithEnabledMarkets()
        {
            var disabledMarket = CreateMarketImpl("myMarket", "myMarket", false);

            IEnumerable<IMarket> markets = new MarketImpl[] { _currentMarket, disabledMarket };
            _mockMarketService.Setup(x => x.GetAllMarkets()).Returns(markets);
            
            var subject = CreateController();
            var result = subject.Index(_contentLink);
            var model = ((ViewResultBase)result).Model as MarketViewModel;
            CollectionAssert.AreEquivalent(new[] { _currentMarket }, model.Markets.ToList());
        }

        [TestMethod]
        public void Index_ShouldReturnModelWithSortedMarkets()
        {
            var contentLink = new ContentReference(11);
            
            IEnumerable<IMarket> markets = new MarketImpl[] { _currentMarket, _market };
            _mockMarketService.Setup(x => x.GetAllMarkets()).Returns(markets);
            
            var subject = CreateController();
            var result = subject.Index(_contentLink);
            var model = ((ViewResultBase)result).Model as MarketViewModel;
            Assert.AreEqual<MarketImpl>( _currentMarket, (MarketImpl)model.Markets.ToList().First());
        }
        
        [TestMethod]
        public void Index_ShouldReturnPartialViewResultType()
        {
            var contentLink = new ContentReference(11);
            var subject = CreateController();
            var result = subject.Index(contentLink);
            Assert.IsInstanceOfType(result, typeof(PartialViewResult));
        }

        [TestMethod]
        public void Index_ShouldReturnModelType()
        {
            var contentLink = new ContentReference(11);
            var subject = CreateController();
            var result = subject.Index(contentLink);
            Assert.IsInstanceOfType((result as ViewResultBase).Model, typeof(MarketViewModel));
        }

        private Mock<ICurrentMarket> _mockCurrentMarket;
        private Mock<IMarketService> _mockMarketService;
        private Mock<UrlResolver> _mockUrlResolver;
        private Mock<LanguageService> _mockLanguageService;
        private MarketImpl _currentMarket;
        private MarketImpl _market;
        private string _language;
        private ContentReference _contentLink;
        
        [TestInitialize]
        public void Setup()
        {
            _contentLink = new ContentReference(11);
            _language = "en";
            _currentMarket = CreateMarketImpl( "currentMarket", "currentMarket", true);
            _market = CreateMarketImpl("myMarket", "myMarket", true);
                        
            _mockCurrentMarket = new Mock<ICurrentMarket>();
            _mockMarketService = new Mock<IMarketService>();
            _mockUrlResolver = new Mock<UrlResolver>();

            _mockLanguageService = new Mock<LanguageService>(null, null, null, null);
            _mockLanguageService.Setup(x => x.SetCurrentLanguage(It.IsAny<string>())).Returns(true);
        }

        private MarketController CreateController()
        {
            return new MarketController(_mockMarketService.Object, _mockCurrentMarket.Object, _mockUrlResolver.Object, _mockLanguageService.Object);
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
