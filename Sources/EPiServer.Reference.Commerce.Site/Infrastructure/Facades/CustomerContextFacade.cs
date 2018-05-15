using EPiServer.ServiceLocation;
using Mediachase.Commerce.Customers;
using System;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.Facades
{
    [ServiceConfiguration(typeof(CustomerContextFacade), Lifecycle = ServiceInstanceScope.Singleton)]
    public class CustomerContextFacade
    {
        private readonly MapUserKey _mapUserKey;

        public CustomerContextFacade(MapUserKey mapUserKey)
        {
            CurrentContact = new CurrentContactFacade();
            _mapUserKey = mapUserKey;
        }
        public virtual CurrentContactFacade CurrentContact { get; }
        public virtual Guid CurrentContactId => CustomerContext.Current.CurrentContactId;

        public virtual CustomerContact GetContactById(Guid contactId)
        {
            return CustomerContext.Current.GetContactById(contactId);
        }

        public virtual CustomerContact GetContactByUsername(string username)
        {
            return CustomerContext.Current.GetContactByUserId(_mapUserKey.ToTypedString(username));
        }
    }
}