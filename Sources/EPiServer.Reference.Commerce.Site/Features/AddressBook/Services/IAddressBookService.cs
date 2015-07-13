using EPiServer.Reference.Commerce.Site.Features.AddressBook.Pages;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using System;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.AddressBook.Services
{
    public interface IAddressBookService
    {
        AddressCollectionViewModel GetAddressBookViewModel(AddressBookPage addressBookPage);
        bool CanSave(Address address);
        void Save(Address viewModel);
        void Delete(Guid addressId);
        void SetPreferredBillingAddress(Guid addressId);
        void SetPreferredShippingAddress(Guid addressId);
        void LoadAddress(Address address);
        void GetCountriesAndRegionsForAddress(Address address);
        IEnumerable<CountryDto.StateProvinceRow> GetRegionOptionsByCountryCode(string countryCode);
        void MapModelToCustomerAddress(Address viewModel, CustomerAddress customerAddress);
        void MapModelToOrderAddress(Address viewModel, OrderAddress orderAddress);
        void MapOrderAddressToModel(Address viewModel, OrderAddress orderAddress);
        void MapCustomerAddressToModel(Address address, CustomerAddress customerAddress);
    }
}
