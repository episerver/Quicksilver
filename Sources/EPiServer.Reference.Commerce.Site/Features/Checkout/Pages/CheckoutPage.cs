using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Pages
{
    [ContentType(DisplayName = "Checkout page", GUID = "6709cd32-7bb6-4d29-9b0b-207369799f4f", Description = "", AvailableInEditMode = false)]
    [AvailableContentTypes(Include = new [] { typeof(OrderConfirmationPage) }, IncludeOn = new [] {typeof(StartPage)})]
    [ImageUrl("~/styles/images/page_type.png")]
    public class CheckoutPage : PageData
    {
    }
}