using System.Collections.Generic;
using EPiServer.ServiceLocation;
using Mediachase.BusinessFoundation.Data;
using Mediachase.Commerce.Customers;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.Facades
{
    [ServiceConfiguration(typeof(CurrentContactFacade), Lifecycle = ServiceInstanceScope.Singleton)]
    public class CurrentContactFacade
    {
        public virtual CustomerContact CurrentContact => CustomerContext.Current.CurrentContact;

        public virtual IEnumerable<CustomerAddress> ContactAddresses
        {
            get
            {
                return CustomerContext.Current.CurrentContact == null 
                    ? Enumerable.Empty<CustomerAddress>() 
                    : CustomerContext.Current.CurrentContact.ContactAddresses;
            }
        }

        public virtual CustomerAddress PreferredBillingAddress
        {
            get { return CustomerContext.Current.CurrentContact.PreferredBillingAddress; }
            set { CustomerContext.Current.CurrentContact.PreferredBillingAddress = value; }
        }

        public virtual PrimaryKeyId? PreferredBillingAddressId
        {
            get { return CustomerContext.Current.CurrentContact.PreferredBillingAddressId; }
            set { CustomerContext.Current.CurrentContact.PreferredBillingAddressId = value; }
        }

        public virtual CustomerAddress PreferredShippingAddress
        {
            get { return CustomerContext.Current.CurrentContact.PreferredShippingAddress; }
            set { CustomerContext.Current.CurrentContact.PreferredShippingAddress = value; }
        }

        public virtual PrimaryKeyId? PreferredShippingAddressId
        {
            get { return CustomerContext.Current.CurrentContact.PreferredShippingAddressId; }
            set { CustomerContext.Current.CurrentContact.PreferredShippingAddressId = value; }
        }

        public virtual void SaveChanges()
        {
            CustomerContext.Current.CurrentContact.SaveChanges();
        }

        public virtual void AddContactAddress(CustomerAddress address)
        {
            CustomerContext.Current.CurrentContact.AddContactAddress(address);
        }

        public virtual void UpdateContactAddress(CustomerAddress address)
        {
            CustomerContext.Current.CurrentContact.UpdateContactAddress(address);
        }

        public virtual void DeleteContactAddress(CustomerAddress address)
        {
            CustomerContext.Current.CurrentContact.DeleteContactAddress(address);
        }
    }
}