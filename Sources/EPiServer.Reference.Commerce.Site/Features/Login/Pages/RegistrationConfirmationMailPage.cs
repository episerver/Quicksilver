using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Reference.Commerce.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.Login.Pages
{
    [ContentType(
        DisplayName = "Registration and confirmation mail page",
        GUID = "85b681d5-20d0-4cf1-9b3f-bbd624c3072e",
        Description = "",
        AvailableInEditMode = false)]
    [AvailableContentTypes(Availability = Availability.None)]
    public class RegistrationConfirmationMailPage : MailBasePage
    {
        [CultureSpecific]
        [Display(
            Name = "Main body",
            Description = "The main body will be shown in the main content area of the page, using the XHTML-editor you can insert for example text, images and tables.",
            GroupName = SystemTabNames.Content,
            Order = 2)]
        public virtual XhtmlString MailBody { get; set; }
    }
}