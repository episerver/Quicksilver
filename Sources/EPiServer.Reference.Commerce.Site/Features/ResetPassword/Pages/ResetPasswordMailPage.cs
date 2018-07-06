using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Reference.Commerce.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.ResetPassword.Pages
{
    [ContentType(DisplayName = "Reset password e-mail", GUID = "6c939f47-b8bb-44a8-bc10-371b54d609b4", Description = "The page which allows you to reset password via e-mail.", AvailableInEditMode = true)]
    [ImageUrl("~/styles/images/page_type.png")]
    public class ResetPasswordMailPage : MailBasePage
    {
        [CultureSpecific]
        [Display(
            Name = "Main body",
            Description = "The main body will be shown in the main content area of the page, using the XHTML-editor you can insert for example text, images and tables.",
            GroupName = SystemTabNames.Content,
            Order = 10)]
        public virtual XhtmlString MailBody { get; set; }
    }
}