using EPiServer.Reference.Commerce.Site.Features.Registration.Blocks;

namespace EPiServer.Reference.Commerce.Site.Features.Registration.Models
{
    public class OrderConfirmationRegistrationModel
    {
        public OrderConfirmationRegistrationFormModel FormModel { get; set; }
        public OrderConfirmationRegistrationBlock CurrentBlock { get; set; }
    }
}