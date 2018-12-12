using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace EPiServer.Commerce.OrderStore.Model
{
    public class ShipmentModel
    {
        public string ShippingMethodName { get; set; }

        public OrderAddressModel ShippingAddress { get; set; }

        public string ShipmentTrackingNumber { get; set; }

        [EnumDataType(typeof(OrderShipmentStatusModel))]
        public OrderShipmentStatusModel OrderShipmentStatus { get; set; }

        public string WarehouseCode { get; set; }

        public List<LineItemModel> LineItems { get; set; }

        [JsonConverter(typeof(HashTableJsonConverter))]
        public Hashtable Properties { get; set; }

        public int? PickListId { get; set; }

        public Guid ShippingMethodId { get; set; }

        public decimal ShippingCost { get; set; }

        public bool IsShippingCostUpToDate { get; set; }

        public decimal ShippingTax { get; set; }

        public bool IsShippingTaxUpToDate { get; set; }

        public decimal ShipmentDiscount { get; set; }

        public IDictionary<int, IEnumerable<string>> OperationKeys { get; set; }

        public int ShipmentId { get; set; }
    }
}