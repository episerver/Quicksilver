using System;

namespace EPiServer.Reference.Commerce.Site.Features.Registration.Models
{
    public class OrderConfirmationRegistrationFormModelBase
    {
        public int OrderNumber { get; set; }
        public Guid ContactId { get; set; }
    }
}