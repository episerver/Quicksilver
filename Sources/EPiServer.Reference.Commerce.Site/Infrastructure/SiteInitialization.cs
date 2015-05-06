using EPiServer.Commerce.Routing;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Framework.Web;
using EPiServer.Reference.Commerce.Site.Features.Market;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Mvc.Html;
using Mediachase.Commerce;
using Mediachase.Search;
using Mediachase.Search.Extensions.Azure;
using StructureMap.Configuration.DSL;
using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;

namespace EPiServer.Reference.Commerce.Site.Infrastructure
{
    [ModuleDependency(typeof(AzureInitialization), typeof(EPiServer.Commerce.Initialization.InitializationModule))]
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

            RegisterDisplayOptions(context);
            
            AreaRegistration.RegisterAllAreas();
        }

        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            Assembly[] assembliesToScan =
                {
                    Assembly.GetExecutingAssembly()
                };
            var registryTypes = assembliesToScan.SelectMany(a => a.GetTypes())
                .Where(t => typeof(Registry).IsAssignableFrom(t))
                .ToArray();

            context.Container.Configure(c =>
            {
                foreach (var type in registryTypes)
                {
                    c.AddRegistry((Registry) Activator.CreateInstance(type));
                }
                c.For<ICurrentMarket>().Singleton().Use<CurrentMarket>();
                //Swap out the default ContentRenderer for our custom
                c.For<ContentAreaRenderer>().Use<ColumnContentAreaRenderer>();
            });

            DependencyResolver.SetResolver(new StructureMapDependencyResolver(context.Container));
        }

        public void Uninitialize(InitializationEngine context) { }

        private void RegisterDisplayOptions(InitializationEngine context)
        {
            if (context.HostType == HostType.WebApplication)
            {
                var options = ServiceLocator.Current.GetInstance<DisplayOptions>();
                options
                    .Add("full", "/displayoptions/full", Constants.ContentAreaTags.FullWidth, "", "epi-icon__layout--full")
                    .Add("wide", "/displayoptions/wide", Constants.ContentAreaTags.TwoThirdsWidth, "", "epi-icon__layout--two-thirds")
                    .Add("narrow", "/displayoptions/narrow", Constants.ContentAreaTags.OneThirdWidth, "", "epi-icon__layout--one-third");
            }
        }
    }
}