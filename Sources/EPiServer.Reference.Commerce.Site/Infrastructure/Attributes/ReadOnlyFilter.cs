using EPiServer.Data;
using EPiServer.ServiceLocation;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.Attributes
{
    public class ReadOnlyFilter : ActionFilterAttribute
    {
        private Injected<IDatabaseMode> _databaseMode = default(Injected<IDatabaseMode>);

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var viewResult = filterContext.Result as ViewResultBase;
            if (viewResult != null)
            {
                viewResult.ViewData["IsReadOnly"] = false;
                if (_databaseMode.Service != null)
                {
                    viewResult.ViewData["IsReadOnly"] = _databaseMode.Service.DatabaseMode == DatabaseMode.ReadOnly;
                }
            }

            base.OnActionExecuted(filterContext);
        }
    }
}