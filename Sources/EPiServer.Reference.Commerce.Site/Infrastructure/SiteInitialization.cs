using EPiServer.Commerce.Routing;
using EPiServer.Editor;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Framework.Web;
using EPiServer.Globalization;
using EPiServer.Reference.Commerce.Shared.Models.Identity;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Indexing;
using EPiServer.Reference.Commerce.Site.Infrastructure.WebApi;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Mediachase.Commerce;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Security;
using Mediachase.Commerce.Website.Helpers;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using StructureMap.Web;
using System;
using System.Globalization;
using System.Web;
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

                c.For<Func<string, CartHelper>>()
                .HybridHttpOrThreadLocalScoped()
                .Use(() => new Func<string, CartHelper>((cartName) => new CartHelper(cartName, PrincipalInfo.CurrentPrincipal.GetContactId())));

                //Register for auto injection of edit mode check, should be default life cycle (per request)
                c.For<Func<bool>>()
                .Use(() => new Func<bool>(() => PageEditing.PageIsInEditMode));                 

                c.For<IUpdateCurrentLanguage>()
                    .Singleton()
                    .Use<LanguageService>()
                    .Setter<IUpdateCurrentLanguage>()
                    .Is(x => x.GetInstance<UpdateCurrentLanguage>());

                c.For<Func<CultureInfo>>().Use(() => new Func<CultureInfo>(() => ContentLanguage.PreferredCulture));

                Func<IOwinContext> owinContextFunc = () => HttpContext.Current.GetOwinContext();
                c.For<ApplicationUserManager>().Use(() => owinContextFunc().GetUserManager<ApplicationUserManager>());
                c.For<ApplicationSignInManager>().Use(() => owinContextFunc().Get<ApplicationSignInManager>());
                c.For<IAuthenticationManager>().Use(() => owinContextFunc().Authentication);
                c.For<IOwinContext>().Use(() => owinContextFunc());
                c.For<IModelBinderProvider>().Use<ModelBinderProvider>();
                c.For<SiteContext>().HybridHttpOrThreadLocalScoped().Use<CustomCurrencySiteContext>();
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