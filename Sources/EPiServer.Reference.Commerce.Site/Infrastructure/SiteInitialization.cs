using EPiServer.Commerce.Order;
using EPiServer.Commerce.Routing;
using EPiServer.Editor;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Framework.Web;
using EPiServer.Globalization;
using EPiServer.Recommendations.Widgets;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using EPiServer.Reference.Commerce.Site.Infrastructure.Business;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Reference.Commerce.Site.Infrastructure.WebApi;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Mediachase.Commerce;
using Mediachase.Commerce.Core;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
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
            GlobalFilters.Filters.Add(new ReadOnlyFilter());

            context.Locate.Advanced.GetInstance<IDisplayChannelService>().RegisterDisplayMode(new DefaultDisplayMode(RenderingTags.Mobile)
            {
                ContextCondition = r => r.GetOverriddenBrowser().IsMobileDevice
            });

            AreaRegistration.RegisterAllAreas();

            DisablePromotionTypes(context);

            SetupExcludedPromotionEntries(context);

            //This method creates and activates the default Recommendations widgets.
            //It only needs to run once, not every initialization, and only if you use the Recommendations feature.
            //Instructions:
            //* Enter the configuration values for Recommendations in web.config
            //* Make sure that the episerver:RecommendationsSilentMode flag is not set to true.
            //* Uncomment the following line, compile, start site, commment the line again, compile.

            //SetupRecommendationsWidgets(context);
        }

        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            var services = context.Services;

            services.AddSingleton<ICurrentMarket, CurrentMarket>();

            //Register for auto injection of edit mode check, should be default life cycle (per request to service locator)
            services.AddTransient<IsInEditModeAccessor>(locator => () => PageEditing.PageIsInEditMode);

            services.Intercept<IUpdateCurrentLanguage>(
                (locator, defaultImplementation) =>
                    new LanguageService(
                        locator.GetInstance<ICurrentMarket>(),
                        locator.GetInstance<CookieService>(),
                        defaultImplementation));

            services.AddTransient<IOrderGroupCalculator, SiteOrderGroupCalculator>();
            services.AddTransient<IOrderFormCalculator, SiteOrderFormCalculator>();
            services.AddTransient<IModelBinderProvider, ModelBinderProvider>();
            services.AddHttpContextOrThreadScoped<SiteContext, CustomCurrencySiteContext>();
            services.AddTransient<HttpContextBase>(locator => HttpContext.Current.ContextBaseOrNull());

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

        private void DisablePromotionTypes(InitializationEngine context)
        {
            //var promotionTypeHandler = context.Locate.Advanced.GetInstance<PromotionTypeHandler>();

            // To disable one of built-in promotion types, for example the BuyQuantityGetFreeItems promotion, comment out the following codes:
            //promotionTypeHandler.DisablePromotions(new[] { typeof(BuyQuantityGetFreeItems) });

            // To disable all built-in promotion types, comment out the following codes:
            //promotionTypeHandler.DisableBuiltinPromotions();
        }

        private void SetupExcludedPromotionEntries(InitializationEngine context)
        {
            //To exclude some entries from promotion engine we need an implementation of IEntryFilter.
            //In most cases you can just use EntryFilterSettings to configure the default implementation. Otherwise you can create your own implementation of IEntryFilter if needed.

            //var filterSettings = context.Locate.Advanced.GetInstance<EntryFilterSettings>();
            //filterSettings.ClearFilters();

            //Add filter predicates for a whole content type.
            //filterSettings.AddFilter<TypeThatShouldNeverBeIncluded>(x => false);

            //Add filter predicates based on any property of the content type, including implemented interfaces.
            //filterSettings.AddFilter<IInterfaceThatCanBeImplementedToDetermineExclusion>(x => !x.ShouldBeExcluded);

            //Add filter predicates based on meta fields that are not part of the content type models, e.g. if the field is dynamically added to entries in an import or integration.
            //filterSettings.AddFilter<EntryContentBase>(x => !(bool)(x["ShouldBeExcludedPromotionMetaField"] ?? false));

            //Add filter predicates base on codes like below.
            //var ExcludingCodes = new string[] { "SKU-36127195", "SKU-39850363", "SKU-39101253" };
            //filterSettings.AddFilter<EntryContentBase>(x => !ExcludingCodes.Contains(x.Code));
        }

        private void SetupRecommendationsWidgets(InitializationEngine context)
        {
            var configuration = context.Locate.Advanced.GetInstance<Recommendations.Configuration>();

            if (configuration.SilentMode)
            {
                return;
            }

            var widgetService = context.Locate.Advanced.GetInstance<WidgetService>();
            var response = widgetService.CreateWidgets();

            if (response.Status != "OK")
            {
                var error = response.Errors.First();
                var message = new StringBuilder($"Code: {error.Code}, Message: {error.Error}");
                
                if (error.Field != null)
                {
                    message.Append($", Field: {error.Field}");
                }

                throw new Exception(message.ToString());
            }

            foreach (var widget in response.EpiPerPage.Pages.SelectMany(x => x.Widgets))
            {
                widget.Active = true;
                var success = widgetService.UpdateWidget(widget);

                if (!success)
                {
                    throw new Exception($"Failed to activate widget {widget.WidgetName}");
                }
            }
        }
    }
}