using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace EPiServer.Commerce.OrderStore.Model
{
    public class PaymentPlanDocument : PaymentPlanModel
    {
        [JsonProperty("id")]
        public string OrderGroupId { get; set; }
    }
}
