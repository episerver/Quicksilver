using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Reference.Commerce.Site.Features.Shared.Pages;

namespace EPiServer.Reference.Commerce.Site.Features.ErrorHandling.Pages
{
    [ContentType(
        DisplayName = "Error page", 
        GUID = "E7DCAABC-05BC-4230-A2AF-94CD9A9ED535", 
        Description = "The page which allows you to show errors details.", 
        AvailableInEditMode = true)]
    [ImageUrl("~/styles/images/page_type.png")]
    public class ErrorPage : StandardPage
    {
    }
}