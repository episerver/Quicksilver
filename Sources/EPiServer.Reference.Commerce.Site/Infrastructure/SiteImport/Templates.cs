using System;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.SiteImport.Templates
{
    [Serializable]
    public class CustomerTemplate
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IEnumerable<AddressTemplate> Addresses { get; set; }
        public IEnumerable<OrderGroupTemplate> Carts { get; set; }
        public IEnumerable<OrderGroupTemplate> PurchaseOrders { get; set; }
    }

    [Serializable]
    public class AddressTemplate
    {
        public Guid AddressId { get; set; }
        public string City { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string Line1 { get; set; }
        public string RegionCode { get; set; }
        public string RegionName { get; set; }
        public string State { get; set; }
    }

    [Serializable]
    public class OrderGroupTemplate
    {
        public IEnumerable<OrderDetails> Details { get; set; }
    }

    [Serializable]
    public class OrderDetails
    {
        public string SKU { get; set; }
        public decimal Quantity { get; set; }
    }
}