using System;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Models
{
    public class ShippingMethodInfoModel
    {
        public Guid MethodId { get; set; }
        public string ClassName { get; set; }
        public string LanguageId { get; set; }
        public string Currency { get; set; }
        public int Ordering { get; set; }
    }
}