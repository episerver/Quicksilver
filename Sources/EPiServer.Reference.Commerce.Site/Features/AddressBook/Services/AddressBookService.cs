using EPiServer.Reference.Commerce.Site.Features.AddressBook.Pages;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.ServiceLocation;
using Mediachase.BusinessFoundation.Data;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.AddressBook.Services
{
    [ServiceConfiguration(typeof(IAddressBookService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class AddressBookService : IAddressBookService
    {
        private static readonly IEnumerable<CountryDto.StateProvinceRow> _emptyRegionList = Enumerable.Empty<CountryDto.StateProvinceRow>();
        private readonly CustomerContextFacade _customercontext;
        private readonly CountryManagerFacade _countryManager;

        public AddressBookService(CustomerContextFacade customerContext, CountryManagerFacade countryManager)
        {
            _customercontext = customerContext;
            _countryManager = countryManager;
        }

        public void MapCustomerAddressToModel(Address address, CustomerAddress customerAddress)
        {
            address.Line1 = customerAddress.Line1;
            address.Line2 = customerAddress.Line2;
            address.City = customerAddress.City;
            address.CountryName = customerAddress.CountryName;
            address.CountryCode = customerAddress.CountryCode;
            address.Email = customerAddress.Email;
            address.FirstName = customerAddress.FirstName;
            address.LastName = customerAddress.LastName;
            address.PostalCode = customerAddress.PostalCode;
            address.SaveAddress = HttpContext.Current.User.Identity.IsAuthenticated;
            address.Region = customerAddress.RegionName ?? customerAddress.State;
            address.ShippingDefault = customerAddress.Equals(_customercontext.CurrentContact.PreferredShippingAddress);
            address.BillingDefault = customerAddress.Equals(_customercontext.CurrentContact.PreferredBillingAddress);
            address.AddressId = customerAddress.AddressId;
            address.Modified = customerAddress.Modified;
            address.Name = customerAddress.Name;
            address.DaytimePhoneNumber = customerAddress.DaytimePhoneNumber;
        }

        public void MapOrderAddressToModel(Address address, OrderAddress orderAddress)
        {
            address.Line1 = orderAddress.Line1;
            address.Line2 = orderAddress.Line2;
            address.City = orderAddress.City;
            address.CountryName = orderAddress.CountryName;
            address.CountryCode = orderAddress.CountryCode;
            address.Email = orderAddress.Email;
            address.FirstName = orderAddress.FirstName;
            address.LastName = orderAddress.LastName;
            address.PostalCode = orderAddress.PostalCode;
            address.SaveAddress = false;
            address.Region = orderAddress.RegionName ?? orderAddress.State;
            address.Modified = orderAddress.Modified;
            address.Name = orderAddress.Name;
            address.DaytimePhoneNumber = orderAddress.DaytimePhoneNumber;
        }

        public void MapModelToOrderAddress(Address address, OrderAddress orderAddress)
        {
            orderAddress.City = address.City;
            orderAddress.CountryCode = address.CountryCode;
            orderAddress.CountryName = GetAllCountries().Where(x => x.Code == address.CountryCode).Select(x => x.Name).FirstOrDefault();
            orderAddress.FirstName = address.FirstName;
            orderAddress.LastName = address.LastName;
            orderAddress.Line1 = address.Line1;
            orderAddress.Line2 = address.Line2;
            orderAddress.DaytimePhoneNumber = address.DaytimePhoneNumber;
            orderAddress.PostalCode = address.PostalCode;
            orderAddress.RegionName = address.Region;
            // Commerce Manager expects State to be set for addresses in order management. Set it to be same as
            // RegionName to avoid issues.
            orderAddress.State = address.Region;
            orderAddress.Email = address.Email;
        }

        public void MapModelToCustomerAddress(Address address, CustomerAddress customerAddress)
        {
            customerAddress.Name = address.Name;
            customerAddress.City = address.City;
            customerAddress.CountryCode = address.CountryCode;
            customerAddress.CountryName = GetAllCountries().Where(x => x.Code == address.CountryCode).Select(x => x.Name).FirstOrDefault();
            customerAddress.FirstName = address.FirstName;
            customerAddress.LastName = address.LastName;
            customerAddress.Line1 = address.Line1;
            customerAddress.Line2 = address.Line2;
            customerAddress.DaytimePhoneNumber = address.DaytimePhoneNumber;
            customerAddress.PostalCode = address.PostalCode;
            customerAddress.RegionName = address.Region;
            // Commerce Manager expects State to be set for addresses in order management. Set it to be same as
            // RegionName to avoid issues.
            customerAddress.State = address.Region;
            customerAddress.Email = address.Email;
            customerAddress.AddressType =
                CustomerAddressTypeEnum.Public |
                (address.ShippingDefault ? CustomerAddressTypeEnum.Shipping : 0) |
                (address.BillingDefault ? CustomerAddressTypeEnum.Billing : 0);
        }

        public AddressCollectionViewModel GetAddressBookViewModel(AddressBookPage addressBookPage)
        {
            var model = new AddressCollectionViewModel
            {
                CurrentPage = addressBookPage,
                Addresses = _customercontext.CurrentContact.ContactAddresses.Select(x => ConvertAddress(x, addressBookPage))
            };
            return model;
        }

        public bool CanSave(Address address)
        {
            return !_customercontext.CurrentContact.ContactAddresses.Any(x =>
                x.Name.Equals(address.Name, StringComparison.InvariantCultureIgnoreCase) &&
                x.AddressId != address.AddressId);
        }

        public void Save(Address address)
        {
            var currentContact = _customercontext.CurrentContact;
            var customerAddress = CreateOrUpdateCustomerAddress(currentContact, address);

            if (address.BillingDefault)
            {
                currentContact.PreferredBillingAddress = customerAddress;
            }
            else if (currentContact.PreferredBillingAddressId == address.AddressId)
            {
                currentContact.PreferredBillingAddressId = null;
            }

            if (address.ShippingDefault)
            {
                currentContact.PreferredShippingAddress = customerAddress;
            }
            else if (currentContact.PreferredShippingAddressId == address.AddressId)
            {
                currentContact.PreferredShippingAddressId = null;
            }

            currentContact.SaveChanges();
        }

        public void Delete(Guid addressId)
        {
            var currentContact = _customercontext.CurrentContact;
            var customerAddress = GetAddress(currentContact, addressId);
            if (customerAddress == null)
            {
                return;
            }
            if (currentContact.PreferredBillingAddressId == customerAddress.PrimaryKeyId || currentContact.PreferredShippingAddressId == customerAddress.PrimaryKeyId)
            {
                currentContact.PreferredBillingAddressId = currentContact.PreferredBillingAddressId == customerAddress.PrimaryKeyId ? null : currentContact.PreferredBillingAddressId;
                currentContact.PreferredShippingAddressId = currentContact.PreferredShippingAddressId == customerAddress.PrimaryKeyId ? null : currentContact.PreferredShippingAddressId;
                currentContact.SaveChanges();
            }
            currentContact.DeleteContactAddress(customerAddress);
            currentContact.SaveChanges();
        }

        public void SetPreferredBillingAddress(Guid addressId)
        {
            var currentContact = _customercontext.CurrentContact;
            var customerAddress = GetAddress(currentContact, addressId);
            if (customerAddress == null)
            {
                return;
            }
            currentContact.PreferredBillingAddress = customerAddress;
            currentContact.SaveChanges();
        }

        public void SetPreferredShippingAddress(Guid addressId)
        {
            var currentContact = _customercontext.CurrentContact;
            var customerAddress = GetAddress(currentContact, addressId);
            if (customerAddress == null)
            {
                return;
            }
            currentContact.PreferredShippingAddress = customerAddress;
            currentContact.SaveChanges();
        }

        public void LoadAddress(Address address)
        {
            var currentContact = _customercontext.CurrentContact;

            address.CountryOptions = GetAllCountries();

            if (address.CountryCode == null && address.CountryOptions.Any())
            {
                address.CountryCode = address.CountryOptions.First().Code;
            }

            if (address.AddressId.HasValue)
            {
                var existingCustomerAddress = GetAddress(currentContact, address.AddressId);

                if (existingCustomerAddress == null)
                {
                    throw new ArgumentException(string.Format("The address id {0} could not be found.", address.AddressId.Value));
                }

                MapCustomerAddressToModel(address, existingCustomerAddress);
            }

            if (!string.IsNullOrEmpty(address.CountryCode))
            {
                address.RegionOptions = GetRegionOptionsByCountryCode(address.CountryCode);
            }
        }

        public IEnumerable<CountryDto.StateProvinceRow> GetRegionOptionsByCountryCode(string countryCode)
        {
            CountryDto.CountryRow country = _countryManager.GetCountryByCountryCode(countryCode);
            if (country != null)
            {
                return GetRegionOptionsFromCountry(country);
            }
            return Enumerable.Empty<CountryDto.StateProvinceRow>();
        }

        public void GetCountriesAndRegionsForAddress(Address address)
        {
            address.CountryOptions = GetAllCountries();

            //try get country first by code, then by name, then the first in list as final fallback
            var selectedCountry = (GetCountryByCode(address) ?? 
                                   GetCountryByName(address)) ??
                                   address.CountryOptions.FirstOrDefault();

            address.RegionOptions = GetRegionOptionsFromCountry(selectedCountry);
        }

        private CountryDto.CountryRow GetCountryByCode(Address address)
        {
            var selectedCountry = address.CountryOptions.FirstOrDefault(x => x.Code == address.CountryCode);
            if (selectedCountry != null)
            {
                address.CountryName = selectedCountry.Name;
            }
            return selectedCountry;
        }

        private CountryDto.CountryRow GetCountryByName(Address address)
        {
            var selectedCountry = address.CountryOptions.FirstOrDefault(x => x.Name == address.CountryName);
            if (selectedCountry != null)
            {
                address.CountryCode = selectedCountry.Code;
            }
            return selectedCountry;
        }

        private IEnumerable<CountryDto.StateProvinceRow> GetRegionOptionsFromCountry(CountryDto.CountryRow country)
        {
            if (country == null)
            {
                return _emptyRegionList;
            }
            return country.GetStateProvinceRows().ToList();
        }

        private CustomerAddress CreateOrUpdateCustomerAddress(CurrentContactFacade contact, Address address)
        {
            var customerAddress = GetAddress(contact, address.AddressId);
            var isNew = customerAddress == null;
            IEnumerable<PrimaryKeyId> existingId = contact.ContactAddresses.Select(a => a.AddressId).ToList();
            if (isNew)
            {
                customerAddress = CustomerAddress.CreateInstance();
            }

            MapModelToCustomerAddress(address, customerAddress);

            if (isNew)
            {
                contact.AddContactAddress(customerAddress);
            }
            else
            {
                contact.UpdateContactAddress(customerAddress);
            }

            contact.SaveChanges();
            if (isNew)
            {
                customerAddress.AddressId = contact.ContactAddresses
                    .Where(a => !existingId.Contains(a.AddressId))
                    .Select(a => a.AddressId)
                    .Single();
                address.AddressId = customerAddress.AddressId;
            }
            return customerAddress;
        }

        private Address ConvertAddress(CustomerAddress customerAddress, AddressBookPage currentPage)
        {
            Address address = null;

            if (customerAddress != null)
            {
                address = new Address();
                MapCustomerAddressToModel(address, customerAddress);
            }

            return address;
        }

        private CustomerAddress GetAddress(CurrentContactFacade contact, Guid? addressId)
        {
            return addressId.HasValue ?
                contact.ContactAddresses.FirstOrDefault(x => x.AddressId == addressId.GetValueOrDefault()) :
                null;
        }

        private List<CountryDto.CountryRow> GetAllCountries()
        {
            return _countryManager.GetCountries().Country.ToList();
        }
    }
}
