using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System.Web;

namespace EPiServer.Reference.Commerce.Shared.Identity
{
    [ModuleDependency(typeof(EPiServer.Commerce.Initialization.InitializationModule))]
    class OwinInitialization : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            var services = context.Services;
            services.AddTransient<IOwinContext>(locator => HttpContext.Current.GetOwinContext());
            services.AddTransient<ApplicationUserManager<SiteUser>>(locator => locator.GetInstance<IOwinContext>().GetUserManager<ApplicationUserManager<SiteUser>>());
            services.AddTransient<IAuthenticationManager>(locator => locator.GetInstance<IOwinContext>().Authentication);
        }

        public void Initialize(InitializationEngine context)
        {

        }

        public void Uninitialize(InitializationEngine context)
        {

        }
    }
}
