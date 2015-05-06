using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.ResetPassword.Blocks
{
    [ContentType(DisplayName = "ResetPasswordBlock", GUID = "0d388b5b-3591-4168-b78a-e3c368dc48b8", Description = "")]
    public class ResetPasswordBlock : BlockData
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