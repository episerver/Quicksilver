using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;

namespace EPiServer.Reference.Commerce.Site.Features.Folder.Pages
{
    [ContentType(DisplayName = "Folder page", GUID = "1bc8e78b-40cc-4efc-a561-a0bba89b51ac", Description = "A page which allows you to structure pages.")]
    [AvailableContentTypes(IncludeOn = new[] { typeof(StartPage), typeof(FolderPage) })]
    [ImageUrl("~/styles/images/page_type.png")]
    public class FolderPage : PageData
    {
    }
}