using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Models;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Pages;

namespace EPiServer.Reference.Commerce.Site.Features.AddressBook
{
    
    public interface IAddressBookService
    {
        AddressBookViewModel GetViewModel(AddressBookPage addressBookPage);
        void Save(AddressBookFormModel model);
        void Delete(Guid addressId);
        void SetPrimaryBillingAddress(Guid addressId);
        void SetPrimaryShippingAddress(Guid addressId);
        AddressBookFormModel LoadFormModel(AddressBookFormModel formModel);
    }
}
