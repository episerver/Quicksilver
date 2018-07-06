using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.ResetPassword.Pages
{
    [ContentType(DisplayName = "Reset password page", GUID = "05834347-8f4f-48ec-a74c-c46278654a92", Description = "Page for allowing users to reset their passwords. The page must also be set in the StartPage's ResetPasswordPage property.")]
    [ImageUrl("~/styles/images/page_type.png")]
    public class ResetPasswordPage : PageData
    {
        [CultureSpecific]
        [Display(
            Name = "Main body",
            Description = "The main body will be shown in the main content area of the page, using the XHTML-editor you can insert for example text, images and tables.",
            GroupName = SystemTabNames.Content,
            Order = 10)]

        public virtual XhtmlString MainBody { get; set; }
    }
}