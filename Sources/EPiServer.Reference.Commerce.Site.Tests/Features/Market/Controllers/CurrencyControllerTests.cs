using System.Linq;
using System.Web.Mvc;
using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Market.Controllers;
using FluentAssertions;
using Mediachase.Commerce;
using Moq;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Market.ViewModels;
using Xunit;


namespace EPiServer.Reference.Commerce.Site.Tests.Features.Market.Controllers
{
    public class CurrencyControllerTests
    {
        [Fact]
        public void Index_ShouldReturnCorrectTypes()
        {
            var result = _subject.Index();

            Assert.IsAssignableFrom<ViewResultBase>(result);
            Assert.IsType<CurrencyViewModel>((result as ViewResultBase).Model);
        }

        [Fact]
        public void Index_ShouldReturnCorrectModel()
        {
            _currencyServiceMock.Setup(x => x.GetCurrentCurrency()).Returns(new Currency("USD"));
            _currencyServiceMock.Setup(x => x.SetCurrentCurrency("USD")).Returns(true);
            _currencyServiceMock.Setup(x => x.GetAvailableCurrencies())
                .Returns(new[] { new Currency("USD"), new Currency("SEK") });

            var result = _subject.Index();

            var model = ((ViewResultBase)result).Model as CurrencyViewModel;

            Assert.Equal<Currency>(new Currency("USD"), model.CurrencyCode);
            model.Currencies.Should().BeEquivalentTo(new[] 
            { 
                new Currency("USD"), 
                new Currency("SEK") 
            }
            .Select(x => new SelectListItem
            { 
                Text = x.CurrencyCode, 
                Value = x.CurrencyCode 
            }));
        }

        [Fact]
        public void Set_WhenInValidCurrency_ShouldReturnHttpError()
        {
            var result = _subject.Set("UNKNOWN CURRENCY");

            Assert.Equal<int>(400, ((HttpStatusCodeResult) result).StatusCode);
        }

        private Mock<ICurrencyService> _currencyServiceMock;
        private Mock<ICartService> _cartService;
        private CurrencyController _subject;


        public CurrencyControllerTests()
        {
            _currencyServiceMock = new Mock<ICurrencyService>();
            _cartService = new Mock<ICartService>();
            _subject = new CurrencyController(_currencyServiceMock.Object, _cartService.Object, new Mock<IOrderRepository>().Object);
        }
    }
}
