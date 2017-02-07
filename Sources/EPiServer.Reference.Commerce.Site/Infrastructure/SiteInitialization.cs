using EPiServer.Commerce.Order;
using EPiServer.Commerce.Routing;
using EPiServer.Editor;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Framework.Web;
using EPiServer.Globalization;
using EPiServer.Reference.Commerce.Shared.Models.Identity;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using EPiServer.Reference.Commerce.Site.Infrastructure.Business;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Reference.Commerce.Site.Infrastructure.WebApi;
using EPiServer.Reference.Customization;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Mediachase.Commerce;
using Mediachase.Commerce.Core;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
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

            services.AddTransient<IOrderGroupCalculator, SiteOrderGroupCalculator>(); // TODO: should remove this configuration and calculator class after COM-2434 was resolved
            services.AddTransient<IOrderFormCalculator, SiteOrderFormCalculator>(); // TODO: should remove this configuration and calculator class after COM-2434 was resolved

            
            services.AddTransient<IOwinContext>(locator => HttpContext.Current.GetOwinContext());
            services.AddTransient<ApplicationUserManager>(locator => locator.GetInstance<IOwinContext>().GetUserManager<ApplicationUserManager>());
            services.AddTransient<ApplicationSignInManager>(locator => locator.GetInstance<IOwinContext>().Get<ApplicationSignInManager>());
            services.AddTransient<IAuthenticationManager>(locator => locator.GetInstance<IOwinContext>().Authentication);

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

            services.AddTransient<ILineItemValidator, CustomizedLineItemValidator>();
            services.AddTransient<ITaxCalculator, CustomizedTaxCalculator>();
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
    }
}