using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Shared.Services
{
    [TestClass]
    public class ControllerExceptionHandlerTests
    {
        [TestMethod]
        public void HandleRequestValidationException_WhenExceptionHasBeenHandled_ShouldNotCallDelegate()
        {
            Setup_exception_has_been_handled();
            Setup_exception(new HttpRequestValidationException());

            var handler = CreateHandler();
            handler.HandleRequestValidationException(
                _exceptionContext,
                "someAction",
                context => { throw new Exception("The delegate should not be called."); });
        }

        [TestMethod]
        public void HandleRequestValidationException_WhenExceptionIsntHttpRequestValidationException_ShouldNotCallDelegate()
        {
            Setup_exception(new Exception());

            var handler = CreateHandler();
            handler.HandleRequestValidationException(
                _exceptionContext,
                "someAction",
                context => { throw new Exception("The delegate should not be called."); });
        }

        [TestMethod]
        public void HandleRequestValidationException_WhenActionDoesNotEqualsActionName_ShouldNotCallDelegate()
        {
            Setup_exception(new HttpRequestValidationException());
            Setup_action_for_controller("otherAction");
            
            var handler = CreateHandler();
            handler.HandleRequestValidationException(
                _exceptionContext,
                "someAction",
                context => { throw new Exception("The delegate should not be called."); });
        }

        [TestMethod]
        public void HandleRequestValidationException_WhenActionEuqalsActionName_ShouldCallDelegate()
        {
            const string actionName = "someAction";
            var delegateResult = new JsonResult();

            Setup_exception(new HttpRequestValidationException());
            Setup_action_for_controller(actionName);

            var handler = CreateHandler();
            handler.HandleRequestValidationException(
                _exceptionContext,
                actionName,
                context => delegateResult);

            Assert.AreEqual<ActionResult>(delegateResult, _exceptionContext.Result);
        }

        [TestMethod]
        public void HandleRequestValidationException_WhenDelegateReturnsNull_ShouldNotSetExceptionHandled()
        {
            const string actionName = "someAction";

            Setup_exception(new HttpRequestValidationException());
            Setup_action_for_controller(actionName);

            var handler = CreateHandler();
            handler.HandleRequestValidationException(
                _exceptionContext,
                actionName,
                context => null);

            Assert.IsFalse(_exceptionContext.ExceptionHandled);
        }

        [TestMethod]
        public void HandleRequestValidationException_WhenDelegateReturnsEmptyResult_ShouldNotSetExceptionHandled()
        {
            const string actionName = "someAction";

            Setup_exception(new HttpRequestValidationException());
            Setup_action_for_controller(actionName);

            var handler = CreateHandler();
            handler.HandleRequestValidationException(
                _exceptionContext,
                actionName,
                context => new EmptyResult());

            Assert.IsFalse(_exceptionContext.ExceptionHandled);
        }

        [TestMethod]
        public void HandleRequestValidationException_WhenDelegateReturnsActionResult_ShouldSetExceptionHandled()
        {
            const string actionName = "someAction";

            Setup_exception(new HttpRequestValidationException());
            Setup_action_for_controller(actionName);

            var handler = CreateHandler();
            handler.HandleRequestValidationException(
                _exceptionContext,
                actionName,
                context => new JsonResult());

            Assert.IsTrue(_exceptionContext.ExceptionHandled);
        }

        Mock<HttpRequestBase> _httpRequestBase;
        Mock<HttpContextBase> _httpContextBase;
        Mock<RequestContext> _requestContext;
        ExceptionContext _exceptionContext;

        [TestInitialize]
        public void Setup()
        {
            _requestContext = new Mock<RequestContext>();
            _httpRequestBase = new Mock<HttpRequestBase>();

            _httpContextBase = new Mock<HttpContextBase>();
            _httpContextBase.Setup(x => x.Request).Returns(_httpRequestBase.Object);

            _exceptionContext = new ExceptionContext
            {
                HttpContext = _httpContextBase.Object,
                RequestContext = _requestContext.Object
            };
        }

        private ControllerExceptionHandler CreateHandler()
        {
            return new ControllerExceptionHandler();
        }

        private void Setup_exception_has_been_handled()
        {
            _exceptionContext.ExceptionHandled = true;
        }

        private void Setup_action_for_controller(string actionName)
        {
            var routeData = new RouteData();
            routeData.Values.Add("action", actionName);

            _exceptionContext.RouteData = routeData;
        }

        private void Setup_exception(Exception exception)
        {
            _exceptionContext.Exception = exception;
        }
    }
}
