using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EPiServer.Commerce.OrderStore.Model
{
    public class OrderGroupModel
    {
        [Required]
        [EnsureMinimumElements(1)]
        public List<OrderFormModel> Forms { get; set; }

        public List<OrderNoteModel> Notes { get; set; }

        public string Market { get; set; }

        [Required]
        public string MarketId { get; set; }

        [Required]
        public string Name { get; set; }

        public Guid? Organization { get; set; }

        [EnumDataType(typeof(OrderStatusModel))]
        public OrderStatusModel OrderStatus { get; set; }

        public string Currency { get; set; }

        public Guid CustomerId { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Modified { get; set; }

        [JsonConverter(typeof(HashTableJsonConverter))]
        public Hashtable Properties { get; set; }

        public decimal TaxTotal { get; set; }

        public bool IsTaxTotalUpToDate { get; set; }

        public string MarketName { get; set; }

        public bool PricesIncludeTax { get; set; }
    }
}
