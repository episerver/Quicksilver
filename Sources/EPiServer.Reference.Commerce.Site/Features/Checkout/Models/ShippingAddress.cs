using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Models
{
    public class ShippingAddress : Address
    {
        public Guid ShippingMethodId { get; set; }
    }
}