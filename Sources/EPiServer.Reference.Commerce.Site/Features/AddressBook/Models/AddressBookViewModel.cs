using System.Collections.Generic;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Pages;

namespace EPiServer.Reference.Commerce.Site.Features.AddressBook.Models
{
    public class AddressBookViewModel
    {
        public AddressBookPage CurrentPage { get; set; }
        public IEnumerable<AddressBookFormModel> Addresses { get; set; }
    }
}