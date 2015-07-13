using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.OrderHistory.Pages
{
    [ContentType(DisplayName = "Order history page", GUID = "6b950185-7270-43bf-90e5-fc57cc0d1b5c", Description = "", AvailableInEditMode = false)]
    public class OrderHistoryPage : PageData
    {
        [CultureSpecific]
        [Display(
            Name = "Main body",
            Description = "The main body will be shown in the main content area of the page, using the XHTML-editor you can insert for example text, images and tables.",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual XhtmlString MainBody { get; set; }
    }
}