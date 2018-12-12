using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace EPiServer.Commerce.OrderStore.Model
{
    public class OrderDocument : OrderModel
    {
        [JsonProperty("id")]
        public string OrderGroupId { get; set; }

        public bool IsInventoryReserved { get; set; }
    }
}
