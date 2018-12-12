using System.Collections;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace EPiServer.Commerce.OrderStore.Model
{
    public class LineItemModel
    {
        public int LineItemId { get; set; }

        public string Code { get; set; }

        public string DisplayName { get; set; }

        public decimal PlacedPrice { get; set; }

        public decimal Quantity { get; set; }

        public decimal ReturnQuantity { get; set; }

        [EnumDataType(typeof(InventoryTrackingStatusModel))]
        public InventoryTrackingStatusModel InventoryTrackingStatus { get; set; }

        public bool IsInventoryAllocated { get; set; }

        public bool IsGift { get; set; }

        [JsonConverter(typeof(HashTableJsonConverter))]
        public Hashtable Properties { get; set; }

        public int? TaxCategoryId { get; set; }
        
        public decimal SalesTax { get; set; }

        public bool IsSalesTaxUpToDate { get; set; }

        public decimal EntryAmount { get; set; }

        public decimal OrderAmount { get; set; }

        public bool AllowBackordersAndPreorders { get; set; }
         
        public decimal InStockQuantity { get; set; }
         
        public decimal BackorderQuantity { get; set; }
         
        public decimal PreorderQuantity { get; set; }
         
        public int InventoryStatus { get; set; }
         
        public decimal MaxQuantity { get; set; }
         
        public decimal MinQuantity { get; set; }

        public bool PricesIncludeTax { get; set; }
    }
}