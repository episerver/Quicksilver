using System;
using System.Web;
using System.Web.Mvc;
using EPiServer.ServiceLocation;
using EPiServer.Data;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Services
{
    [ServiceConfiguration(Lifecycle = ServiceInstanceScope.Singleton)]
    public class ControllerExceptionHandler
    {
        private Injected<IDatabaseMode> _databaseMode = default(Injected<IDatabaseMode>);

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

            // Handle the read-only flag so that it works even in the exception-handling scenario
            var viewResult = filterContext.Result as ViewResultBase;
            if (viewResult != null)
            {
                viewResult.ViewData["IsReadOnly"] = false;
                if (_databaseMode.Service != null)
                {
                    viewResult.ViewData["IsReadOnly"] = _databaseMode.Service.DatabaseMode == DatabaseMode.ReadOnly;
                }
            }

            if (filterContext.Result != null && !(filterContext.Result is EmptyResult))
            {
                filterContext.ExceptionHandled = true;
            }
        }
    }
}