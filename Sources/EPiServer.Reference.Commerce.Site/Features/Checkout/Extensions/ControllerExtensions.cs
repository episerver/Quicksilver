using System.Web.Mvc;
using System.Web.Routing;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Extensions
{
    public static class ControllerExtensions
    {
        public static ActionResult ExecuteAction(this IController controller, string actionName, string controllerName = null, object values = null)
        {
            var routeValueDictionary = new RouteValueDictionary(values);
            if (!string.IsNullOrEmpty(actionName)) routeValueDictionary["action"] = actionName;
            if (!string.IsNullOrEmpty(controllerName)) routeValueDictionary["controller"] = controllerName;
            return new ExecuteActionResult(routeValueDictionary);
        }
    }
}