using EPiServer.ServiceLocation;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.Facades
{
    [ServiceConfiguration(typeof(CustomerContextFacade), Lifecycle = ServiceInstanceScope.Singleton)]
    public class CustomerContextFacade
    {
        public CustomerContextFacade()
        {
            CurrentContact = new CurrentContactFacade();
        }
        public virtual CurrentContactFacade CurrentContact { get; private set; }
    }
}