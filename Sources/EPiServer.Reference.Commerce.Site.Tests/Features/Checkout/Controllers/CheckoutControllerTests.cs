using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Payment.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Web.Routing;
using Mediachase.Commerce.Website.Helpers;
using Moq;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Services;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using Mediachase.Commerce.Orders;
using Xunit;


namespace EPiServer.Reference.Commerce.Site.Tests.Features.Checkout.Controllers
{
    public class CheckoutControllerTests
    {
        [Fact]
        public void OnException_ShouldDelegateToExceptionHandler()
        {
            var testController = CreateTestController();
            testController.CallOnException(_exceptionContext);

            _controllerExceptionHandlerMock.Verify(x => x.HandleRequestValidationException(_exceptionContext, "purchase", testController.OnPurchaseException));
        }

        [Fact]
        public void OnPurchaseException_WhenRoutedDataIsntSet_ShouldReturnEmptyResult()
        {
            Setup_RequestContext_to_contain_routed_data(null);
            Setup_exception(new HttpRequestValidationException());

            var result = _subject.OnPurchaseException(_exceptionContext);
            Assert.IsType(typeof(EmptyResult), result);
        }

        Mock<HttpRequestBase> _httpRequestBaseMock;
        Mock<HttpContextBase> _httpContextBaseMock;
        Mock<RequestContext> _requestContextMock;
        ExceptionContext _exceptionContext;
        Mock<ControllerExceptionHandler> _controllerExceptionHandlerMock;
        CheckoutController _subject;


        public CheckoutControllerTests()
        {
            _controllerExceptionHandlerMock = new Mock<ControllerExceptionHandler>();
            _requestContextMock = new Mock<RequestContext>();
            _httpRequestBaseMock = new Mock<HttpRequestBase>();

            _httpContextBaseMock = new Mock<HttpContextBase>();
            _httpContextBaseMock.Setup(x => x.Request).Returns(_httpRequestBaseMock.Object);

            _exceptionContext = new ExceptionContext
            {
                HttpContext = _httpContextBaseMock.Object,
                RequestContext = _requestContextMock.Object
            };

            _subject = new CheckoutController(null, null, null, null, null, null, null, null, null, null, null, _controllerExceptionHandlerMock.Object, null, null);
        }

        private CheckoutControllerForTest CreateTestController()
        {
            return new CheckoutControllerForTest(null, null, null, null, null, null, null, null, null, null, null, _controllerExceptionHandlerMock.Object, null, null);
        }

        private void Setup_RequestContext_to_contain_routed_data(object rotedData)
        {
            var routeData = new RouteData();
            routeData.DataTokens.Add(RoutingConstants.RoutedDataKey, rotedData);

            _requestContextMock.Setup(x => x.RouteData).Returns(routeData);
        }

        private void Setup_exception(Exception exception)
        {
            _exceptionContext.Exception = exception;
        }

        private class CheckoutControllerForTest : CheckoutController
        {
            public CheckoutControllerForTest(
                ICartService cartService, 
                IContentRepository contentRepository, 
                UrlResolver urlResolver, 
                IMailService mailService, 
                ICheckoutService checkoutService, 
                IContentLoader contentLoader, 
                IPaymentService paymentService, 
                LocalizationService localizationService, 
                Func<string, CartHelper> cartHelper, 
                ICurrencyService currencyService, 
                IAddressBookService addressBookService, 
                ControllerExceptionHandler controllerExceptionHandler, 
                CustomerContextFacade customerContextFacade,
                CookieService cookieService)
                : base(cartService, 
                      contentRepository, 
                      urlResolver, 
                      mailService, 
                      checkoutService, 
                      contentLoader, 
                      paymentService, 
                      localizationService, 
                      cartHelper, 
                      currencyService, 
                      addressBookService, 
                      controllerExceptionHandler, 
                      customerContextFacade,
                      cookieService)
            {
            }

            public void CallOnException(ExceptionContext filterContext)
            {
                OnException(filterContext);
            }
        }
    }
}
