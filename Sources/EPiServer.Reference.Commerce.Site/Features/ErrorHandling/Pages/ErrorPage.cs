using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.ErrorHandling.Pages
{
    [ContentType(
        DisplayName = "Error page", 
        GUID = "E7DCAABC-05BC-4230-A2AF-94CD9A9ED535", 
        Description = "The page which allows you to show errors details.", 
        AvailableInEditMode = true)]
    [ImageUrl("~/styles/images/page_type.png")]
    public class ErrorPage :PageData
    {
        [CultureSpecific]
        [Display(
               Name = "Title",
               Description = "Title for the page",
               GroupName = SystemTabNames.Content,
               Order = 1)]
        public virtual string Title { get; set; }

        [CultureSpecific]
        [Display(
               Name = "Main body",
               Description = "Main body",
               GroupName = SystemTabNames.Content,
               Order = 2)]
        public virtual XhtmlString MainBody { get; set; } 
    }
}