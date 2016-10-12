using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using Mediachase.BusinessFoundation.Data;
using Mediachase.Commerce.Customers;

namespace EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes
{
    class FakeCurrentContact : CurrentContactFacade
    {
        private readonly List<CustomerAddress> _addresses;
        private CustomerAddress _preferredBillingAddress;
        private CustomerAddress _preferredShippingAddress;

        public FakeCurrentContact(IEnumerable<CustomerAddress> addresses)
        {
            _addresses = addresses.ToList();
        }

        public override IEnumerable<CustomerAddress> ContactAddresses
        {
            get { return _addresses; }
        }

        public override CustomerAddress PreferredBillingAddress
        {
            get
            {
                return _preferredBillingAddress;
            }
            set
            {
                _preferredBillingAddress = value;

                if (value == null)
                {
                    PreferredBillingAddressId = null;
                }
                else
                {
                    PreferredBillingAddressId = value.AddressId;
                }
            }
        }

        public override PrimaryKeyId? PreferredBillingAddressId { get; set; }

        public override CustomerAddress PreferredShippingAddress
        {
            get
            {
                return _preferredShippingAddress;
            }
            set
            {
                _preferredShippingAddress = value;

                if (value == null)
                {
                    PreferredShippingAddressId = null;
                }
                else
                {
                    PreferredShippingAddressId = value.AddressId;
                }
            }
        }

        public override PrimaryKeyId? PreferredShippingAddressId { get; set; }

        public override void SaveChanges()
        {
            _addresses.ForEach(x =>
            {
                if (string.IsNullOrEmpty(x.Name))
                {
                    x.AddressId = new PrimaryKeyId(Guid.NewGuid());
                    x.Name = x.AddressId.ToString();
                }
            });
        }

        public override void AddContactAddress(CustomerAddress address)
        {
            address.AddressId = new PrimaryKeyId(Guid.Parse(address.Name));
            _addresses.Add(address);
        }

        public override void UpdateContactAddress(CustomerAddress address)
        {
            var addressToUpdate = _addresses.FirstOrDefault(x => x.AddressId == address.AddressId);
            if (addressToUpdate != null)
            {
                addressToUpdate = address;
            }
        }

        public override void DeleteContactAddress(CustomerAddress address)
        {
            var addressToDelete = _addresses.FirstOrDefault(x => x.AddressId == address.AddressId);
            if (addressToDelete != null)
            {
                _addresses.Remove(addressToDelete);
            }
        }
    }
}