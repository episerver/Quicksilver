using EPiServer.Reference.Commerce.Site.Infrastructure;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
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
    }
}