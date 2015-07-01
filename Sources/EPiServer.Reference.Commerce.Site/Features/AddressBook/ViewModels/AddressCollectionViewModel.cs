using EPiServer.Reference.Commerce.Site.Features.AddressBook.Pages;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.AddressBook.ViewModels
{
    public class AddressCollectionViewModel
    {
        public AddressBookPage CurrentPage { get; set; }
        public IEnumerable<Address> Addresses { get; set; }
    }
}