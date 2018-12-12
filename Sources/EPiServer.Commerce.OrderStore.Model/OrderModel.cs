using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EPiServer.Commerce.OrderStore.Model
{
    public class OrderModel : OrderGroupModel
    {
        [Required]
        public string OrderNumber { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public List<ReturnOrderFormModel> ReturnForms { get; set; }
    }
}
