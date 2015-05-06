using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.Search.Pages
{
    [ContentType(DisplayName = "SearchPage", GUID = "6e0c84de-bd17-43ee-9019-04f08c7fcf8d", Description = "", AvailableInEditMode = false)]
    public class SearchPage : PageData
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