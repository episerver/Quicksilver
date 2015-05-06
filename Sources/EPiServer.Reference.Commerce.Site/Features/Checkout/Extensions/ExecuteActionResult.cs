using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using EPiServer.Web.Routing;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Extensions
{
    public class ExecuteActionResult : ActionResult
    {
        private readonly RouteValueDictionary _values;

        public ExecuteActionResult(RouteValueDictionary values)
        {
            _values = values;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var data = _values;

            var currentRoute = context.RequestContext.RouteData;
            var routeData = new RouteData(currentRoute.Route, currentRoute.RouteHandler);
            foreach (var item in currentRoute.Values) routeData.Values.Add(item.Key, item.Value);
            foreach (var item in currentRoute.DataTokens.Where(kvp => !kvp.Key.Equals(RoutingConstants.ControllerTypeKey))) routeData.DataTokens.Add(item.Key, item.Value);
            foreach (var item in data)
            {
                routeData.Values[item.Key] = item.Value;
            }

            var factory = ControllerBuilder.Current.GetControllerFactory();
            var requestContext = new RequestContext(context.HttpContext, routeData);
            var controller = factory.CreateController(requestContext, (string) routeData.Values["controller"]);

            try
            {
                controller.Execute(requestContext);
            }
            finally
            {
                factory.ReleaseController(controller);
            }
        }
    }
}