using EPiServer.Reference.Commerce.Site.Features.AddressBook.Pages;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.ViewModels;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.AddressBook.ViewModels
{
    public class AddressCollectionViewModel : PageViewModel<AddressBookPage>
    {
        public IEnumerable<AddressModel> Addresses { get; set; }
    }
}