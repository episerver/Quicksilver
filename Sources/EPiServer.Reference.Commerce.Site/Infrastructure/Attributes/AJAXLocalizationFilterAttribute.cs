using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.Attributes
{
    public class AJAXLocalizationFilterAttribute : ActionFilterAttribute
    {
        private Injected<IUpdateCurrentLanguage> _currentLanguageUpdater = default(Injected<IUpdateCurrentLanguage>);

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                _currentLanguageUpdater.Service.UpdateLanguage(null);
            }
        }
    }
}