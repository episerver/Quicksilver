using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.Registration.Blocks
{
    [ContentType(DisplayName = "OrderConfirmationRegistrationBlock", GUID = "cb3c314b-1a90-44d5-82f6-a1b6e1d23cb4", Description = "")]
    public class OrderConfirmationRegistrationBlock : BlockData
    {
        /*
                [CultureSpecific]
                [Display(
                    Name = "Name",
                    Description = "Name field's description",
                    GroupName = SystemTabNames.Content,
                    Order = 1)]
                public virtual String Name { get; set; }
         */
    }
}