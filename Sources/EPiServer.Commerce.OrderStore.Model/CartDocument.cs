using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace EPiServer.Commerce.OrderStore.Model
{
    public class CartDocument : CartModel
    {
        [JsonProperty("id")]
        public string OrderGroupId { get; set; }
    }
}
