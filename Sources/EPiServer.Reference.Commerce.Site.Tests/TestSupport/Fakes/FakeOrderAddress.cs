using EPiServer.Commerce.Order;
using System.Collections;

namespace EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes
{
    class FakeOrderAddress : IOrderAddress
    {
        public string Id { get; set; }

        public string City { get; set; }

        public string CountryCode { get; set; }

        public string CountryName { get; set; }

        public string DaytimePhoneNumber { get; set; }

        public string Email { get; set; }

        public string EveningPhoneNumber { get; set; }

        public string FaxNumber { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Line1 { get; set; }

        public string Line2 { get; set; }

        public string Organization { get; set; }

        public string PostalCode { get; set; }

        public string RegionCode { get; set; }

        public string RegionName { get; set; }

        public Hashtable Properties { get; set; }

    }
}