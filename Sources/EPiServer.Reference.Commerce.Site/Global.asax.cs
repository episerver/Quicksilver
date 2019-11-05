using EPiServer.Reference.Commerce.Site.Features.ErrorHandling.Controllers;
using EPiServer.Reference.Commerce.Site.Infrastructure;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.UI;

namespace EPiServer.Reference.Commerce.Site
{
    public class Global : EPiServer.Global
    {
        protected override void RegisterRoutes(RouteCollection routes)
        {
            base.RegisterRoutes(routes);

            routes.MapRoute(
              name: "Default",
              url: "{controller}/{action}/{id}",
              defaults: new { action = "Index", id = UrlParameter.Optional });
        }

        protected void Application_Start()
        {
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(LocalizedRequiredAttribute), typeof(RequiredAttributeAdapter));
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(LocalizedRegularExpressionAttribute), typeof(RegularExpressionAttributeAdapter));
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(LocalizedEmailAttribute), typeof(RegularExpressionAttributeAdapter));
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(LocalizedStringLengthAttribute), typeof(StringLengthAttributeAdapter));

            ScriptManager.ScriptResourceMapping.AddDefinition("jquery", new ScriptResourceDefinition
            {
                Path = "~/Scripts/jquery-1.11.1.js",
            });

            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            if (Context.IsCustomErrorEnabled)
            {
                ShowCustomErrorPage(Server.GetLastError());
            }
        }

        private void ShowCustomErrorPage(Exception exception)
        {
            var httpException = exception as HttpException;
            if (httpException == null)
            {
                httpException = new HttpException(500, "Internal Server Error", exception);
            }

            Response.Clear();
            var routeData = new RouteData();
            routeData.Values.Add("controller", "ErrorHandling");
            routeData.Values.Add("fromAppErrorEvent", true);

            switch (httpException.GetHttpCode())
            {
                case 403:
                    routeData.Values.Add("action", "Forbidden");
                    break;

                case 404:
                    routeData.Values.Add("action", "PageNotFound");
                    break;

                case 500:
                    routeData.Values.Add("action", "InternalError");
                    break;

                default:
                    routeData.Values.Add("action", "OtherHttpStatusCode");
                    routeData.Values.Add("httpStatusCode", httpException.GetHttpCode());
                    break;
            }

            Server.ClearError();

            IController controller = new ErrorHandlingController();
            controller.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
        }

        protected void Application_EndRequest()
        {
            if (Context.Response.StatusCode == 302
                && (Context.User == null || !Context.User.Identity.IsAuthenticated)
                && Context.Request.Path.ToString().ToLower().Contains("/api/"))
            {
                Context.Response.StatusCode = 401;
            }
        }
    }
}