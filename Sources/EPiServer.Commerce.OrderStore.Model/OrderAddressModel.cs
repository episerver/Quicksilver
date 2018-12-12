using System.Collections;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace EPiServer.Commerce.OrderStore.Model
{
    public class OrderAddressModel
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Organization { get; set; }

        public string Line1 { get; set; }

        public string Line2 { get; set; }

        public string City { get; set; }

        public string CountryCode { get; set; }

        public string CountryName { get; set; }

        public string PostalCode { get; set; }

        public string RegionCode { get; set; }

        public string RegionName { get; set; }

        public string DaytimePhoneNumber { get; set; }

        public string EveningPhoneNumber { get; set; }

        public string FaxNumber { get; set; }

        public string Email { get; set; }

        [JsonConverter(typeof(HashTableJsonConverter))]
        public Hashtable Properties { get; set; }
    }
}