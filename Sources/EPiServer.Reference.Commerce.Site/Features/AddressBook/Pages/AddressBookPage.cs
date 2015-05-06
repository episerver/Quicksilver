using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.AddressBook.Pages
{
    [ContentType(DisplayName = "AdressBookPage", GUID = "5e373eb0-7930-45ca-8564-e695aacd83b4", Description = "", AvailableInEditMode = false)]
    public class AddressBookPage : PageData
    {
        /*
                [CultureSpecific]
                [Display(
                    Name = "Main body",
                    Description = "The main body will be shown in the main content area of the page, using the XHTML-editor you can insert for example text, images and tables.",
                    GroupName = SystemTabNames.Content,
                    Order = 1)]
                public virtual XhtmlString MainBody { get; set; }
         */
    }
}