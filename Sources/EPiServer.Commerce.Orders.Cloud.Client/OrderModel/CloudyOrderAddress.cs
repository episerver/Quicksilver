using EPiServer.Commerce.Serialization.Json;
using Newtonsoft.Json;
using System;
using System.Collections;
using EPiServer.Commerce.Order.Internal;

namespace EPiServer.Commerce.Orders.Cloud.Client.OrderModel
{
    [Serializable]
    public class CloudyOrderAddress : IShippingOrderAddress
    {
        string _city;
        string _countryCode;
        string _postalCode;
        string _regionCode;

        [JsonConverter(typeof(HashTableJsonConverter))]
        public Hashtable Properties { get; set; }

        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Organization { get; set; }

        public string Line1 { get; set; }

        public string Line2 { get; set; }

        public string City
        {
            get => _city;
            set
            {
                _city = value;
                ShippingAddressChanged();
            }
        }

        public string CountryCode
        {
            get => _countryCode;
            set
            {
                _countryCode = value;
                ShippingAddressChanged();
            }
        }

        public string CountryName { get; set; }

        public string PostalCode
        {
            get => _postalCode;
            set
            {
                _postalCode = value;
                ShippingAddressChanged();
            }
        }

        public string RegionCode
        {
            get => _regionCode;
            set
            {
                _regionCode = value;
                ShippingAddressChanged();
            }
        }

        public string RegionName { get; set; }

        public string DaytimePhoneNumber { get; set; }

        public string EveningPhoneNumber { get; set; }

        public string FaxNumber { get; set; }

        public string Email { get; set; }

        [JsonIgnore]
        public IShipmentCalculatedAmount Shipment { get; set; }

        [JsonConstructor]
        public CloudyOrderAddress()
        {
            Properties = new Hashtable();
        }

        public void ShippingAddressChanged()
        {
            Shipment?.ResetUpToDateFlags();
        }
    }
}