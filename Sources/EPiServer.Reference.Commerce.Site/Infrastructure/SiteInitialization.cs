using EPiServer.Commerce.Routing;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Framework.Web;
using EPiServer.Globalization;
using EPiServer.Reference.Commerce.Site.Features.Market;
using EPiServer.Reference.Commerce.Site.Infrastructure.WebApi;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Security;
using Mediachase.Commerce.Website.Helpers;
using Newtonsoft.Json;
using System;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;

namespace EPiServer.Reference.Commerce.Site.Infrastructure
{
    [ModuleDependency(typeof(EPiServer.Commerce.Initialization.InitializationModule))]
    public class SiteInitialization : IConfigurableModule
    {
        public void Initialize(InitializationEngine context)
        {
            CatalogRouteHelper.MapDefaultHierarchialRouter(RouteTable.Routes, false);
            
            GlobalFilters.Filters.Add(new HandleErrorAttribute());

            context.Locate.DisplayChannelService().RegisterDisplayMode(new DefaultDisplayMode(RenderingTags.Mobile)
            {
                ContextCondition = r => r.GetOverriddenBrowser().IsMobileDevice
            });
            
            AreaRegistration.RegisterAllAreas();
            
        }

        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Container.Configure(c =>
            {
                c.For<ICurrentMarket>().Singleton().Use<CurrentMarket>();

                c.For<Func<CartHelper>>()
                .HybridHttpOrThreadLocalScoped()
                .Use(() => new CartHelper(Cart.DefaultName, PrincipalInfo.CurrentPrincipal.GetContactId()));

                c.For<IUpdateCurrentLanguage>()
                    .Singleton()
                    .Use<LanguageService>()
                    .Setter<IUpdateCurrentLanguage>()
                    .Is(x => x.GetInstance<UpdateCurrentLanguage>());
            });

            DependencyResolver.SetResolver(new StructureMapDependencyResolver(context.Container));
            GlobalConfiguration.Configure(config =>
            {
                config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.LocalOnly;
                config.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings();
                config.Formatters.XmlFormatter.UseXmlSerializer = true;
                config.DependencyResolver = new StructureMapResolver(context.Container);
                config.MapHttpAttributeRoutes();
            });
        }

        public void Uninitialize(InitializationEngine context) { }
    }
}