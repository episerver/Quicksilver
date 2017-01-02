using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Pages
{
    [ContentType(DisplayName = "Multi shipment page", 
        GUID = "6D084537-84D0-48C2-8770-80995374D9DA", 
        Description = "Page for dividing the customer's cart items into multiple shipments.", 
        AvailableInEditMode = true)]

    [ImageUrl("~/styles/images/page_type.png")]
    public class MultiShipmentPage : PageData
	{
	}
}