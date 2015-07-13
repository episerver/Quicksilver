using EPiServer.ServiceLocation;
using Mediachase.Commerce.Customers;
using System;

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
        public virtual Guid CurrentContactId { get { return CustomerContext.Current.CurrentContactId;} }
        public virtual CustomerContact GetContactById(Guid contactId)
        {
            return CustomerContext.Current.GetContactById(contactId);
        }
    }
}