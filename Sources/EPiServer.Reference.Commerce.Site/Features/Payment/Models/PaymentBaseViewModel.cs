using System;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.Models
{
    public class PaymentBaseViewModel
    {
        public Guid PaymentMethodId { get; set; }
        public virtual string Controller { get; set; }
    }
}