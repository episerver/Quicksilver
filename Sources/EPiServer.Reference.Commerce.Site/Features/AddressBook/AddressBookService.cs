using System.Collections.Generic;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Models;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Pages;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Customers;
using System;
using System.Linq;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;

namespace EPiServer.Reference.Commerce.Site.Features.AddressBook
{
    [ServiceConfiguration(typeof(IAddressBookService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class AddressBookService : IAddressBookService
    {
        
        public AddressBookViewModel GetViewModel(AddressBookPage addressBookPage)
        {
            var currentContact = CustomerContext.Current.CurrentContact;
            var preferredShippingId = currentContact.PreferredShippingAddress != null ? currentContact.PreferredShippingAddress.AddressId : Guid.Empty;
            var preferredBillingId = currentContact.PreferredBillingAddress != null ? currentContact.PreferredBillingAddress.AddressId : Guid.Empty;
            var model = new AddressBookViewModel
            {
                CurrentPage = addressBookPage,
                Addresses = currentContact.ContactAddresses.Select(x => ConvertAddress(x, x.AddressId == preferredShippingId, x.AddressId == preferredBillingId, addressBookPage)),
            };
            return model;
        }

        public void Save(AddressBookFormModel model)
        {
            var currentContact = CustomerContext.Current.CurrentContact;

            var isNew = !currentContact.ContactAddresses.Any(x => x.AddressId == model.AddressId);

            var address = CreateOrUpdateCustomerAddress(model);

            if (isNew)
            {
                currentContact.AddContactAddress(address);
            }
            else
            {
                currentContact.UpdateContactAddress(address);
            }

            currentContact.SaveChanges();
            address.AddressId = currentContact.ContactAddresses
                .Single(x => x.Name.Equals(address.Name)).AddressId; 

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
            var currentContact = CustomerContext.Current.CurrentContact;
            var address = currentContact.ContactAddresses.FirstOrDefault(x => x.AddressId == addressId);
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

        public void SetPrimaryBillingAddress(Guid addressId)
        {
            var currentContact = CustomerContext.Current.CurrentContact;
            var address = currentContact.ContactAddresses.FirstOrDefault(x => x.AddressId == addressId);
            if (address == null)
            {
                return;
            }
            currentContact.PreferredBillingAddress = address;
            currentContact.SaveChanges();
        }

        public void SetPrimaryShippingAddress(Guid addressId)
        {
            var currentContact = CustomerContext.Current.CurrentContact;
            var address = currentContact.ContactAddresses.FirstOrDefault(x => x.AddressId == addressId);
            if (address == null)
            {
                return;
            }
            currentContact.PreferredShippingAddress = address;
            currentContact.SaveChanges();
        }

        private CustomerAddress CreateOrUpdateCustomerAddress(AddressBookFormModel model)
        {
            var address = CustomerContext.Current.CurrentContact.ContactAddresses.FirstOrDefault(x => x.AddressId == model.AddressId) ??
                          CustomerAddress.CreateInstance();

            address.Name = model.Name;
            address.FirstName = model.FirstName;
            address.LastName = model.LastName;
            address.Line1 = model.Line1;
            address.Line2 = model.Line2;
            address.PostalCode = model.PostalCode;
            address.City = model.City;
            address.CountryName = model.CountryName;
            address.Email = model.Email;
            address.DaytimePhoneNumber = model.DaytimePhoneNumber;
            address.RegionName = model.Region;

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
                Region = address.RegionName
            };
        }


        public AddressBookFormModel LoadFormModel(AddressBookFormModel formModel)
        {
            var countries = CountryManager.GetCountries().Country.ToList();
            if (!formModel.AddressId.HasValue)
            {
                return new AddressBookFormModel 
                {
                    CurrentPage = formModel.CurrentPage,
                    CountryOptions = countries,
                    RegionOptions = countries.First().GetStateProvinceRows().ToList()
                };
            }

            var currentContact = CustomerContext.Current.CurrentContact;
            var existingAddress = currentContact.ContactAddresses.FirstOrDefault(x => x.AddressId == formModel.AddressId.GetValueOrDefault());
            if (existingAddress == null)
            {
                return new AddressBookFormModel
                {
                    CurrentPage = formModel.CurrentPage,
                    CountryOptions = countries,
                    RegionOptions = countries.First().GetStateProvinceRows().ToList()
                };
            }
            formModel.City = existingAddress.City;
            
            if (String.IsNullOrEmpty(formModel.CountryName) || formModel.CountryName.Equals(existingAddress.CountryName))
            { 
                formModel.CountryName = existingAddress.CountryName;
            }
            formModel.CountryOptions = countries;
            formModel.ShippingDefault = currentContact.PreferredShippingAddress != null && existingAddress.Equals(currentContact.PreferredShippingAddress);
            formModel.BillingDefault = currentContact.PreferredBillingAddressId != null && existingAddress.Equals(currentContact.PreferredBillingAddress);
            formModel.FirstName = existingAddress.FirstName;
            formModel.LastName = existingAddress.LastName;
            formModel.Line1 = existingAddress.Line1;
            formModel.Name = existingAddress.Name;
            formModel.PostalCode = existingAddress.PostalCode;
            formModel.Email = existingAddress.Email;
            formModel.DaytimePhoneNumber = existingAddress.DaytimePhoneNumber;
            formModel.Modified = existingAddress.Modified;
            var selectedCountry = countries.FirstOrDefault(x => x.Name.Equals(formModel.CountryName));
            formModel.RegionOptions = selectedCountry == null ? new List<CountryDto.StateProvinceRow>() : selectedCountry.GetStateProvinceRows().ToList();
            if (String.IsNullOrEmpty(formModel.Region) || formModel.Region.Equals(existingAddress.RegionName))
            {
                formModel.Region = existingAddress.RegionName;
            }
            return formModel;
        }
    }
}
