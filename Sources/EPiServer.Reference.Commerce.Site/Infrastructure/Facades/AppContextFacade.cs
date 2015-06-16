using System;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Core;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.Facades
{
    [ServiceConfiguration(typeof(AppContextFacade), Lifecycle = ServiceInstanceScope.Singleton)]
    public class AppContextFacade
    {
        public virtual Guid ApplicationId
        {
            get { return AppContext.Current.ApplicationId; }
        }
    }
}