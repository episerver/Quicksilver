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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Services;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Checkout.Controllers
{
    [TestClass]
    public class CheckoutControllerTests
    {
        [TestMethod]
        public void OnException_ShouldDelegateToExceptionHandler()
        {
            var testController = CreateTestController();
            testController.CallOnException(_exceptionContext);

            _controllerExceptionHandler.Verify(x => x.HandleRequestValidationException(_exceptionContext, "purchase", testController.OnPurchaseException));
        }

        [TestMethod]
        public void OnPurchaseException_WhenRoutedDataIsntSet_ShouldReturnEmptyResult()
        {
            Setup_RequestContext_to_contain_routed_data(null);
            Setup_exception(new HttpRequestValidationException());

            var result = _subject.OnPurchaseException(_exceptionContext);
            Assert.IsInstanceOfType(result, typeof(EmptyResult));
        }

        Mock<HttpRequestBase> _httpRequestBase;
        Mock<HttpContextBase> _httpContextBase;
        Mock<RequestContext> _requestContext;
        ExceptionContext _exceptionContext;
        Mock<ControllerExceptionHandler> _controllerExceptionHandler;
        CheckoutController _subject;

        [TestInitialize]
        public void Setup()
        {
            _controllerExceptionHandler = new Mock<ControllerExceptionHandler>();
            _requestContext = new Mock<RequestContext>();
            _httpRequestBase = new Mock<HttpRequestBase>();

            _httpContextBase = new Mock<HttpContextBase>();
            _httpContextBase.Setup(x => x.Request).Returns(_httpRequestBase.Object);

            _exceptionContext = new ExceptionContext
            {
                HttpContext = _httpContextBase.Object,
                RequestContext = _requestContext.Object
            };

            _subject = new CheckoutController(null, null, null, null, null, null, null, null, null, null,null, _controllerExceptionHandler.Object,null);
        }

        private CheckoutControllerForTest CreateTestController()
        {
            return new CheckoutControllerForTest(null, null, null, null, null, null, null, null, null, null, null, _controllerExceptionHandler.Object, null);
        }

        private void Setup_RequestContext_to_contain_routed_data(object rotedData)
        {
            var routeData = new RouteData();
            routeData.DataTokens.Add(RoutingConstants.RoutedDataKey, rotedData);

            _requestContext.Setup(x => x.RouteData).Returns(routeData);
        }

        private void Setup_exception(Exception exception)
        {
            _exceptionContext.Exception = exception;
        }

        private class CheckoutControllerForTest : CheckoutController
        {
            public CheckoutControllerForTest(ICartService cartService, IContentRepository contentRepository, UrlResolver urlResolver, IMailService mailService, ICheckoutService checkoutService, IContentLoader contentLoader, IPaymentService paymentService, LocalizationService localizationService, Func<string,CartHelper> cartHelper, CurrencyService currencyService, AddressBookService addressBookService, ControllerExceptionHandler controllerExceptionHandler, CustomerContextFacade customerContextFacade)
                : base(cartService, contentRepository, urlResolver, mailService, checkoutService, contentLoader, paymentService, localizationService, cartHelper, currencyService, addressBookService, controllerExceptionHandler,customerContextFacade)
            {
            }

            public void CallOnException(ExceptionContext filterContext)
            {
                OnException(filterContext);
            }
        }
    }
}
