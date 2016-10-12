using System;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.Models
{
    public class PaymentMethodModel
    {
        public string Description { get; set; }
        public string FriendlyName { get; set; }
        public string LanguageId { get; set; }
        public Guid PaymentMethodId { get; set; }
        public string SystemName { get; set; }
    }
}