using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Marketing.Promotions;
using EPiServer.Commerce.Routing;
using EPiServer.Editor;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Framework.Web;
using EPiServer.Globalization;
using EPiServer.Personalization.Commerce;
using EPiServer.Personalization.Commerce.Widgets;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Reference.Commerce.Site.Infrastructure.WebApi;
using EPiServer.ServiceLocation;
using EPiServer.Tracking.Commerce;
using EPiServer.Web;
using EPiServer.Web.Routing;
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
using EPiServer.Personalization.Commerce.Tracking;

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
            GlobalFilters.Filters.Add(new AJAXLocalizationFilterAttribute());

            context.Locate.Advanced.GetInstance<IDisplayChannelService>().RegisterDisplayMode(new DefaultDisplayMode(RenderingTags.Mobile)
            {
                ContextCondition = r => r.GetOverriddenBrowser().IsMobileDevice
            });

            AreaRegistration.RegisterAllAreas();

            context.Locate.Advanced.GetInstance<OrderEventListener>().AddEvents();

#if DISABLE_PROMOTION_TYPES_FEATURE
            DisablePromotionTypes(context);
#endif

#if EXCLUDE_ITEMS_FROM_PROMOTION_ENGINE_FEATURE
            SetupExcludePromotionEntries(context);
#endif

#if ACTIVATE_DEFAULT_PERSONALIZATION_WIDGETS_FEATURE
            SetupPersonalizationsWidgets(context);
#endif
        }

        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            var services = context.Services;

            services.AddSingleton<IRecommendationContext, RecommendationContext>();

            services.AddSingleton<ICurrentMarket, CurrentMarket>();

            services.AddSingleton<ITrackingResponseDataInterceptor, TrackingResponseDataInterceptor>();

            //Register for auto injection of edit mode check, should be default life cycle (per request to service locator)
            services.AddTransient<IsInEditModeAccessor>(locator => () => PageEditing.PageIsInEditMode);

            services.Intercept<IUpdateCurrentLanguage>(
                (locator, defaultImplementation) =>
                    new LanguageService(
                        locator.GetInstance<ICurrentMarket>(),
                        locator.GetInstance<CookieService>(),
                        defaultImplementation));

            services.AddTransient<IModelBinderProvider, ModelBinderProvider>();
            services.AddHttpContextOrThreadScoped<SiteContext, CustomCurrencySiteContext>();
            services.AddTransient<HttpContextBase>(locator => HttpContext.Current.ContextBaseOrNull());

            services.AddSingleton<ServiceAccessor<IContentRouteHelper>>(locator => locator.GetInstance<IContentRouteHelper>);

            DependencyResolver.SetResolver(new StructureMapDependencyResolver(context.StructureMap()));
            GlobalConfiguration.Configure(config =>
            {
                config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.LocalOnly;
                config.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings();
                config.Formatters.XmlFormatter.UseXmlSerializer = true;
                config.DependencyResolver = new StructureMapResolver(context.StructureMap());
                config.MapHttpAttributeRoutes();
            });

#if IRI_CHARACTERS_IN_URL_FEATURE
            EnableIriCharactersInUrls(context);
#endif
        }

        public void Uninitialize(InitializationEngine context)
        {
            context.Locate.Advanced.GetInstance<OrderEventListener>().RemoveEvents();
        }

        /// <summary>
        /// Enables the IRI characters in Urls.
        /// </summary>
        /// <param name="context">The service configuration context.</param>
        /// <remarks>
        /// To use this feature, define IRI_CHARACTERS_IN_URL_FEATURE symbol - this should be done both in Commerce Manager and Front-end sites.
        /// More information about this feature: http://world.episerver.com/documentation/developer-guides/CMS/routing/internationalized-resource-identifiers-iris/
        /// To support more Unicode blocks, update the regular expression for ValidCharacters.
        /// For example, to support Thai Unicode block, add \p{IsThai} to it.
        /// The supported Unicode blocks can be found here: https://msdn.microsoft.com/en-us/library/20bw873z(v=vs.110).aspx#Anchor_12
        /// </remarks>
        private void EnableIriCharactersInUrls(ServiceConfigurationContext context)
        {
            context.Services.RemoveAll<UrlSegmentOptions>();
            context.Services.AddSingleton(s => new UrlSegmentOptions
            {
                SupportIriCharacters = true,
                ValidCharacters = @"\p{L}0-9\-_~\.\$"
            });
        }

        /// <summary>
        /// Disables promotion types.
        /// </summary>
        /// <param name="context">The initialization engine.</param>
        /// <remarks>
        /// To use this feature, define DISABLE_PROMOTION_TYPES_FEATURE symbol.
        /// </remarks>
        private void DisablePromotionTypes(InitializationEngine context)
        {
            var promotionTypeHandler = context.Locate.Advanced.GetInstance<PromotionTypeHandler>();

            // To disable one of the built-in promotion types, for example the BuyQuantityGetFreeItems promotion.
            promotionTypeHandler.DisablePromotions(new[] { typeof(BuyQuantityGetFreeItems) });

            // To disable all built-in promotion types.
            promotionTypeHandler.DisableBuiltinPromotions();
        }

        /// <summary>
        /// Excludes items from promotion engine.
        /// </summary>
        /// <param name="context">The initialization engine.</param>
        /// <remarks>
        /// To use this feature, define EXCLUDE_ITEMS_FROM_PROMOTION_ENGINE_FEATURE symbol.
        /// </remarks>
        private void SetupExcludePromotionEntries(InitializationEngine context)
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

        /// <summary>
        /// Creates and activates the default Personalization widgets.
        /// </summary>
        /// <param name="context">The initialization engine.</param>
        /// <remarks>
        /// To use this feature, define ACTIVATE_DEFAULT_PERSONALIZATION_WIDGETS_FEATURE symbol.
        /// It only needs to run once, not upon every initialization, and only if you use the Recommendations feature.
        /// Instructions:
        ///     Enter the configuration values for Personalization in web.config.
        ///     Make sure that the episerver:tracking.Enabled setting is set to true, and other personalization settings have proper values.
        /// </remarks>
        private void SetupPersonalizationWidgets(InitializationEngine context)
        {
            var configuration = context.Locate.Advanced.GetInstance<PersonalizationClientConfiguration>();

            if (!configuration.TrackingEnabled)
            {
                return;
            }

            foreach (var scope in configuration.GetScopes())
            {
                var widgetService = context.Locate.Advanced.GetInstance<WidgetService>();
                var response = widgetService.CreateWidgets(scope);

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
                    var success = widgetService.UpdateWidget(widget, scope);

                    if (!success)
                    {
                        throw new Exception($"Failed to activate widget {widget.WidgetName}");
                    }
                }
            }
        }
    }
}