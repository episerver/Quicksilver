using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Models;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Pages;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using Mediachase.Commerce.Orders.Dto;

namespace EPiServer.Reference.Commerce.Site.Features.AddressBook
{
    
    public interface IAddressBookService
    {
        AddressBookViewModel GetViewModel(AddressBookPage addressBookPage);
        bool CanSave(AddressBookFormModel model);
        void Save(AddressBookFormModel model);
        void Delete(Guid addressId);
        void SetPreferredBillingAddress(Guid addressId);
        void SetPreferredShippingAddress(Guid addressId);
        AddressBookFormModel LoadFormModel(AddressBookFormModel formModel);
        void UpdateCountrySelection(AddressModelBase formModel);
        IEnumerable<CountryDto.StateProvinceRow> GetRegionOptionsByCountryCode(string countryCode);
    }
}
