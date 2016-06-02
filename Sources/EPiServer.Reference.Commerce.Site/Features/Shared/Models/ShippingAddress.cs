using System;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Models
{
    public class ShippingAddress : Address
    {
        public Guid ShippingMethodId { get; set; }
    }
}