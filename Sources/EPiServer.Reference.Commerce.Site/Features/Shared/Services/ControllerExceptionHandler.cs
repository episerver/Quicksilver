using System;
using System.Web;
using System.Web.Mvc;
using EPiServer.ServiceLocation;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Services
{
    [ServiceConfiguration(Lifecycle = ServiceInstanceScope.Singleton)]
    public class ControllerExceptionHandler
    {
        public virtual void HandleRequestValidationException(ExceptionContext filterContext, string actionName, Func<ExceptionContext, ActionResult> getActionResult)
        {
            if (filterContext.ExceptionHandled || !(filterContext.Exception is HttpRequestValidationException))
            {
                return;
            }

            var routedAction = filterContext.RouteData.Values["action"].ToString();
            if (!routedAction.Equals(actionName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            filterContext.Result = getActionResult(filterContext);
            if (filterContext.Result != null && !(filterContext.Result is EmptyResult))
            {
                filterContext.ExceptionHandled = true;
            }
        }
    }
}