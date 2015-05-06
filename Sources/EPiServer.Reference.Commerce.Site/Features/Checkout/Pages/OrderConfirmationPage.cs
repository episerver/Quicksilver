using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Pages
{
    [ContentType(DisplayName = "Confirmation page", GUID = "04285260-47be-4ecf-9118-558d6c88d3c0", Description = "", AvailableInEditMode = false)]
    [AvailableContentTypes(Availability = Availability.None)]
    public class OrderConfirmationPage : PageData
    {
        [CultureSpecific]
        [Display(
            Name = "Title",
            Description = "",
            GroupName = SystemTabNames.Content,
            Order = 10)]
        public virtual string Title { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Body text",
            Description = "",
            GroupName = SystemTabNames.Content,
            Order = 20)]
        public virtual XhtmlString Body { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Registration area",
            Description = "",
            GroupName = SystemTabNames.Content,
            Order = 30)]
        public virtual ContentArea RegistrationArea { get; set; }
    }
}