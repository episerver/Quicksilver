using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.ResetPassword.Pages
{
    [ContentType(DisplayName = "PasswordResetLinkPage", GUID = "e4357e92-d178-4211-8919-b42f3dd83e7d", Description = "", AvailableInEditMode = false)]
    public class PasswordResetLinkPage : PageData
    {
        [CultureSpecific]
        [Display(
            Name = "Main title",
            Description = "",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual string MainTitle { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Main body",
            Description = "The main body will be shown in the main content area of the page, using the XHTML-editor you can insert for example text, images and tables.",
            GroupName = SystemTabNames.Content,
            Order = 2)]
        public virtual XhtmlString MainBody { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Done Title",
            Description = "",
            GroupName = SystemTabNames.Content,
            Order = 3)]
        public virtual string DoneTitle { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Done body",
            Description = "The main body will be shown in the main content area of the page, using the XHTML-editor you can insert for example text, images and tables.",
            GroupName = SystemTabNames.Content,
            Order = 4)]
        public virtual XhtmlString DoneBody { get; set; }
    }
}