using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Pages
{
    [ContentType(DisplayName = "Checkout page", GUID = "6709cd32-7bb6-4d29-9b0b-207369799f4f", Description = "", AvailableInEditMode = false)]
    [AvailableContentTypes(Include = new [] { typeof(OrderConfirmationPage), typeof(MultiShipmentPage) }, IncludeOn = new [] {typeof(StartPage)})]
    [ImageUrl("~/styles/images/page_type.png")]
    public class CheckoutPage : PageData
    { 
        [Display(
            Name = "Multishipment page",
            Description = "Page for dividing the customer's cart items into multiple shipments.",
            GroupName = SiteTabs.SiteStructure,
            Order = 1)]
        [AllowedTypes(typeof(MultiShipmentPage))]
        public virtual ContentReference MultiShipmentPage { get; set; }
    }
}