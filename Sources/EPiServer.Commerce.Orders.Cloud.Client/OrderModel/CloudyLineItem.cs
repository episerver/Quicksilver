using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Internal;
using EPiServer.Commerce.Serialization.Json;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.Orders;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Commerce.Orders.Cloud.Client.OrderModel

{
    [Serializable]
#pragma warning disable 612, 618
    public class CloudyLineItem : ILineItem, ILineItemInventory, ILineItemDiscountAmount, ILineItemCalculatedAmount
#pragma warning disable 612, 618
    {
        CloudyOrderGroupBase _parent;
        decimal _quantity;
        decimal _placedPrice;
        decimal _orderAmount;
        decimal _entryAmount;
        decimal _returnQuantity;
        int? _taxCategoryId;
        
        public CloudyLineItem()
        {
            Properties = new Hashtable();
        }

        public int LineItemId { get; set; }

        public string Code { get; set; }

        public string DisplayName { get; set; }

        public decimal PlacedPrice
        {
            get
            {
                return _placedPrice;
            }
            set
            {
                if (_placedPrice != value)
                {
                    _placedPrice = value;
                    ResetUpToDateFlags();
                }
            }
        }

        public decimal Quantity
        {
            get
            {
                return _quantity;
            }
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    ResetUpToDateFlags(includedShippingCost: true);
                }
            }
        }

        public decimal ReturnQuantity
        {
            get
            {
                return _returnQuantity;
            }
            set
            {
                if (_returnQuantity != value)
                {
                    _returnQuantity = value;
                    ResetUpToDateFlags();
                }
            }
        }

        public InventoryTrackingStatus InventoryTrackingStatus
        {
            get
            {
                var status = InventoryTrackingStatus.Disabled;
                if (Enum.IsDefined(typeof(InventoryTrackingStatus), InventoryStatus))
                {
                    status = (InventoryTrackingStatus)InventoryStatus;
                }
                return status;
            }
            set => InventoryStatus = (int)value;
        }

        public bool IsInventoryAllocated { get; set; }

        public bool IsGift { get; set; }

        [JsonConverter(typeof(HashTableJsonConverter))]
        public Hashtable Properties { get; set; }

        public bool AllowBackordersAndPreorders { get; set; }

        public decimal InStockQuantity { get; set; }

        public decimal BackorderQuantity { get; set; }

        public decimal PreorderQuantity { get; set; }

        public int InventoryStatus { get; set; }

        public decimal MaxQuantity { get; set; }

        public decimal MinQuantity { get; set; }

        public decimal EntryAmount
        {
            get
            {
                return _entryAmount;
            }
            set
            {
                if (_entryAmount != value)
                {
                    _entryAmount = value;
                    ResetUpToDateFlags();
                }
            }
        }

        public decimal OrderAmount
        {
            get
            {
                return _orderAmount;
            }
            set
            {
                if (_orderAmount != value)
                {
                    _orderAmount = value;

                    // Changing the line item order amount does not affect calculated shipping tax of parent shipment.
                    ResetUpToDateFlags(false, updatedOrderLevelDiscountAmount:true);
                }
            }
        }

        public int? TaxCategoryId
        {
            get
            {
                return _taxCategoryId;
            }
            set
            {
                if (_taxCategoryId != value)
                {
                    _taxCategoryId = value;
                    ResetUpToDateFlags();
                }
            }
        }

        public decimal SalesTax { get; set; }

        public bool IsSalesTaxUpToDate { get; set; }

        public bool PricesIncludeTax => _parent?.PricesIncludeTax ?? false;

        internal void SetParent(CloudyOrderGroupBase newParent, bool keepIds)
        {
            if (!keepIds && newParent != null && newParent != _parent)
            {
                var forms = new List<IOrderForm>(newParent.Forms);

                if (newParent is IPurchaseOrder order)
                {
                    forms.AddRange(order.ReturnForms);
                }

                LineItemId = forms.SelectMany(x => x.Shipments).SelectMany(y => y.LineItems).Select(z => z.LineItemId).Max() + 1;
            }
            _parent = newParent;
        }

        void ResetUpToDateFlags(bool includeShipmentShippingTax = true, bool includedShippingCost = false, bool updatedOrderLevelDiscountAmount = false)
        {
            //changing the order level discount amount for a return line item should not affect
            //to return taxes.
            if (!updatedOrderLevelDiscountAmount || ReturnQuantity == 0)
            {
                IsSalesTaxUpToDate = false;
            }

            if (_parent == null)
            {
                return;
            }

            _parent.IsTaxTotalUpToDate = false;

            if (!includedShippingCost && !includeShipmentShippingTax)
            {
                return;
            }

            if (_parent.Forms
                    .SelectMany(x => x.Shipments)
                    .FirstOrDefault(s => s.LineItems.Any(l => l.LineItemId == LineItemId)) is IShipmentCalculatedAmount
                shipmentCalculatedPrices)
            {
                if (includeShipmentShippingTax)
                {
                    shipmentCalculatedPrices.IsShippingTaxUpToDate = false;
                }

                if (includedShippingCost)
                {
                    shipmentCalculatedPrices.IsShippingCostUpToDate = false;
                }
            }
        }
    }
}