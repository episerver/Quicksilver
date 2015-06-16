using System.Collections.Generic;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Models;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Pages;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Customers;
using System;
using System.Linq;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.BusinessFoundation.Data;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;

namespace EPiServer.Reference.Commerce.Site.Features.AddressBook
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

        public AddressBookViewModel GetViewModel(AddressBookPage addressBookPage)
        {
            var currentContact = _customercontext.CurrentContact;
            var preferredShippingId = currentContact.PreferredShippingAddress != null ?
                currentContact.PreferredShippingAddress.AddressId :
                Guid.Empty;
            var preferredBillingId = currentContact.PreferredBillingAddress != null ?
                currentContact.PreferredBillingAddress.AddressId :
                Guid.Empty;

            var model = new AddressBookViewModel
            {
                CurrentPage = addressBookPage,
                Addresses = currentContact.ContactAddresses.Select(x =>
                    ConvertAddress(x, x.AddressId == preferredShippingId, x.AddressId == preferredBillingId, addressBookPage)),
            };
            return model;
        }

        public bool CanSave(AddressBookFormModel model)
        {
            return !_customercontext.CurrentContact.ContactAddresses.Any(x =>
                x.Name.Equals(model.Name, StringComparison.InvariantCultureIgnoreCase) &&
                x.AddressId != model.AddressId);
        }

        public void Save(AddressBookFormModel model)
        {
            var currentContact = _customercontext.CurrentContact;
            var address = CreateOrUpdateCustomerAddress(currentContact, model);

            if (model.BillingDefault)
            {
                currentContact.PreferredBillingAddress = address;
            }
            else if (currentContact.PreferredBillingAddressId == model.AddressId)
            {
                currentContact.PreferredBillingAddressId = null;
            }

            if (model.ShippingDefault)
            {
                currentContact.PreferredShippingAddress = address;
            }
            else if (currentContact.PreferredShippingAddressId == model.AddressId)
            {
                currentContact.PreferredShippingAddressId = null;
            }
           
            currentContact.SaveChanges();
        }

        public void Delete(Guid addressId)
        {
            var currentContact = _customercontext.CurrentContact;
            var address = GetAddress(currentContact, addressId);
            if (address == null)
            {
                return;
            }
            if (currentContact.PreferredBillingAddressId == address.PrimaryKeyId || currentContact.PreferredShippingAddressId == address.PrimaryKeyId)
            {
                currentContact.PreferredBillingAddressId = currentContact.PreferredBillingAddressId == address.PrimaryKeyId ? null : currentContact.PreferredBillingAddressId;
                currentContact.PreferredShippingAddressId = currentContact.PreferredShippingAddressId == address.PrimaryKeyId ? null : currentContact.PreferredShippingAddressId;
                currentContact.SaveChanges();
            }
            currentContact.DeleteContactAddress(address);
            currentContact.SaveChanges();
        }

        public void SetPreferredBillingAddress(Guid addressId)
        {
            var currentContact = _customercontext.CurrentContact;
            var address = GetAddress(currentContact, addressId);
            if (address == null)
            {
                return;
            }
            currentContact.PreferredBillingAddress = address;
            currentContact.SaveChanges();
        }

        public void SetPreferredShippingAddress(Guid addressId)
        {
            var currentContact = _customercontext.CurrentContact;
            var address = GetAddress(currentContact, addressId);
            if (address == null)
            {
                return;
            }
            currentContact.PreferredShippingAddress = address;
            currentContact.SaveChanges();
        }

        public AddressBookFormModel LoadFormModel(AddressBookFormModel formModel)
        {
            var countries = GetAllCountries();
            if (!formModel.AddressId.HasValue)
            {
                return CreateFormModel(formModel.CurrentPage, countries);
            }

            var currentContact = _customercontext.CurrentContact;
            var existingAddress = GetAddress(currentContact, formModel.AddressId);
            if (existingAddress == null)
            {
                return CreateFormModel(formModel.CurrentPage, countries);
            }

            formModel.City = existingAddress.City;
            formModel.CountryCode = existingAddress.CountryCode;
            formModel.CountryName = existingAddress.CountryName;
            formModel.ShippingDefault = existingAddress.Equals(currentContact.PreferredShippingAddress);
            formModel.BillingDefault = existingAddress.Equals(currentContact.PreferredBillingAddress);
            formModel.FirstName = existingAddress.FirstName;
            formModel.LastName = existingAddress.LastName;
            formModel.Line1 = existingAddress.Line1;
            formModel.Name = existingAddress.Name;
            formModel.PostalCode = existingAddress.PostalCode;
            formModel.Email = existingAddress.Email;
            formModel.DaytimePhoneNumber = existingAddress.DaytimePhoneNumber;
            formModel.Modified = existingAddress.Modified;
            formModel.Region = existingAddress.RegionName;

            UpdateCountrySelection(formModel);

            return formModel;
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

        public IEnumerable<CountryDto.StateProvinceRow> GetRegionOptionsFromCountry(CountryDto.CountryRow country)
        {
            if (country == null)
            {
                return _emptyRegionList;
            }
            return country.GetStateProvinceRows().ToList();
        }

        public void UpdateCountrySelection(AddressModelBase formModel)
        {
            formModel.CountryOptions = GetAllCountries();
            var selectedCountry = GetCountryByCode(formModel) ?? GetCountryByName(formModel);
            formModel.RegionOptions = GetRegionOptionsFromCountry(selectedCountry);
        }

        private CustomerAddress CreateOrUpdateCustomerAddress(CurrentContactFacade contact, AddressBookFormModel model)
        {
            var address = GetAddress(contact, model.AddressId);
            var isNew = address == null;
            IEnumerable<PrimaryKeyId> existingId = contact.ContactAddresses.Select(a => a.AddressId).ToList();
            if (isNew)
            {
                address = CustomerAddress.CreateInstance();
            }

            address.Name = model.Name;
            address.FirstName = model.FirstName;
            address.LastName = model.LastName;
            address.Line1 = model.Line1;
            address.Line2 = model.Line2;
            address.PostalCode = model.PostalCode;
            address.City = model.City;
            address.CountryName = GetAllCountries().Where(x => x.Code == model.CountryCode).Select(x => x.Name).FirstOrDefault();
            address.CountryCode = model.CountryCode;
            address.Email = model.Email;
            address.DaytimePhoneNumber = model.DaytimePhoneNumber;
            address.RegionName = model.Region;
            // Commerce Manager expects State to be set for addresses in order management. Set it to be same as
            // RegionName to avoid issues.
            address.State = model.Region;
            address.AddressType =
                CustomerAddressTypeEnum.Public |
                (model.ShippingDefault ? CustomerAddressTypeEnum.Shipping : 0) |
                (model.BillingDefault ? CustomerAddressTypeEnum.Billing : 0);

            if (isNew)
            {
                contact.AddContactAddress(address);
            }
            else
            {
                contact.UpdateContactAddress(address);
            }

            contact.SaveChanges();
            if (isNew)
            {
                address.AddressId = contact.ContactAddresses
                    .Where(a => !existingId.Contains(a.AddressId))
                    .Select(a => a.AddressId)
                    .Single();
                model.AddressId = address.AddressId;
            }
            return address;
        }

        private static AddressBookFormModel ConvertAddress(CustomerAddress address, bool defaultShipping, bool defaultBilling, AddressBookPage currentPage)
        {
            if (address == null)
            {
                return null;
            }

            return new AddressBookFormModel
            {
                AddressId = address.AddressId,
                City = address.City,
                CountryCode = address.CountryCode,
                CountryName = address.CountryName,
                FirstName = address.FirstName,
                LastName = address.LastName,
                Line1 = address.Line1,
                Line2 = address.Line2,
                Name = address.Name,
                PostalCode = address.PostalCode,
                BillingDefault = defaultBilling,
                ShippingDefault = defaultShipping,
                Email = address.Email,
                DaytimePhoneNumber = address.DaytimePhoneNumber,
                CurrentPage = currentPage,
                Modified = address.Modified,
                // Commerce Manager uses State in some places where RegionName should be used instead. Here
                // we use State as a fallback if RegionName is not set.
                Region = address.RegionName ?? address.State
            };
        }

        private CountryDto.CountryRow GetCountryByCode(AddressModelBase formModel)
        {
            var selectedCountry = formModel.CountryOptions.FirstOrDefault(x => x.Code == formModel.CountryCode);
            if (selectedCountry != null)
            {
                formModel.CountryName = selectedCountry.Name;
            }
            return selectedCountry;
        }

        private CountryDto.CountryRow GetCountryByName(AddressModelBase formModel)
        {
            var selectedCountry = formModel.CountryOptions.FirstOrDefault(x => x.Name == formModel.CountryName);
            if (selectedCountry != null)
            {
                formModel.CountryCode = selectedCountry.Code;
            }
            return selectedCountry;
        }

        private CustomerAddress GetAddress(CurrentContactFacade contact, Guid? addressId)
        {
            return contact.ContactAddresses.FirstOrDefault(x => x.AddressId == addressId.GetValueOrDefault());
        }

        private List<CountryDto.CountryRow>  GetAllCountries()
        {
            return _countryManager.GetCountries().Country.ToList();
        }

        private AddressBookFormModel CreateFormModel(AddressBookPage page, IEnumerable<CountryDto.CountryRow> countries)
        {
            return new AddressBookFormModel
            {
                CurrentPage = page,
                CountryOptions = countries,
                RegionOptions = GetRegionOptionsFromCountry(countries.FirstOrDefault())
            };
        }
    }
}
