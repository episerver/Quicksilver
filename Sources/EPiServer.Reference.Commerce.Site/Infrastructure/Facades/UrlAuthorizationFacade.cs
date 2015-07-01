using EPiServer.Security;
using EPiServer.ServiceLocation;
using System.Web.Security;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.Facades
{
    [ServiceConfiguration(typeof(UrlAuthorizationFacade), Lifecycle = ServiceInstanceScope.Singleton)]
    public class UrlAuthorizationFacade
    {
        public virtual bool CheckUrlAccessForPrincipal(string path)
        {
            return UrlAuthorizationModule.CheckUrlAccessForPrincipal(path, PrincipalInfo.CurrentPrincipal, "GET");
        }
    }
}