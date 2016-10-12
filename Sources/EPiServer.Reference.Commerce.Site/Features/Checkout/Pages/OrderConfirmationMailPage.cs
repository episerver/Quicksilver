using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Reference.Commerce.Shared.Models;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Pages
{
    [ContentType(DisplayName = "Order and confirmation mail page", GUID = "535070c8-e08b-45ff-9703-c7d990174017", Description = "", AvailableInEditMode = false)]
    public class OrderConfirmationMailPage : MailBasePage
    {
        [CultureSpecific]
        [Display(
            Name = "Main body",
            Description = "The main body will be shown in the main content area of the page, using the XHTML-editor you can insert for example text, images and tables.",
            GroupName = SystemTabNames.Content,
            Order = 2)]
        public virtual XhtmlString MainBody { get; set; }
    }
}