using EPiServer.Reference.Commerce.Site.Features.AddressBook.Pages;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;

namespace EPiServer.Reference.Commerce.Site.Features.AddressBook.ViewModels
{
    public class AddressViewModel
    {
        public AddressBookPage CurrentPage { get; set; }
        public Address Address { get; set; }
    }
}