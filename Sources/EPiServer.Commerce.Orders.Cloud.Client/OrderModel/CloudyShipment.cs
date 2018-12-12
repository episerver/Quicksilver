using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Internal;
using EPiServer.Commerce.Serialization.Json;
using Mediachase.Commerce.Orders;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;

namespace EPiServer.Commerce.Orders.Cloud.Client.OrderModel
{
    [Serializable]
    public class CloudyShipment : IShipment, IShipmentInventory, IShipmentDiscountAmount, IShipmentCalculatedAmount
    {
        ConcurrentDictionary<int, IEnumerable<string>> _operationKeys;
        CloudyOrderGroupBase _parent;
        IOrderAddress _shippingAddress;
        Guid _shippingMethodId;
        decimal _shipmentDiscount;

        public CloudyShipment()
        {
            var observableLineItemCollection = new ObservableCollection<ILineItem>();
            observableLineItemCollection.CollectionChanged += LineItemsChanged;
            LineItems = observableLineItemCollection;

            Properties = new Hashtable();

            _operationKeys = new ConcurrentDictionary<int, IEnumerable<string>>();
            OrderShipmentStatus = OrderShipmentStatus.AwaitingInventory;
        }

        [JsonConstructor]
        public CloudyShipment(ICollection<CloudyLineItem> lineItems, CloudyOrderAddress shippingAddress)
        {
            foreach (var lineItem in lineItems)
            {
                lineItem.SetParent(null, true);
            }
            var observableLineItemCollection = new ObservableCollection<ILineItem>(lineItems);
            observableLineItemCollection.CollectionChanged += LineItemsChanged;
            LineItems = observableLineItemCollection;

            ShippingAddress = shippingAddress;
        }

        public int ShipmentId { get; set; }

        public Guid ShippingMethodId
        {
            get { return _shippingMethodId; }
            set
            {
                if (_shippingMethodId != value)
                {
                    _shippingMethodId = value;
                    ResetUpToDateFlags();
                }
            }
        }

        public string ShippingMethodName { get; set; }

        public IOrderAddress ShippingAddress
        {
            get
            {
                if (_shippingAddress is IShippingOrderAddress shipmentAwareAddress && shipmentAwareAddress.Shipment == null)
                {
                    shipmentAwareAddress.Shipment = this;
                }
                
                return _shippingAddress;
            }
            set
            {
                if (_shippingAddress != value)
                {
                    _shippingAddress = value;
                    if (_shippingAddress is IShippingOrderAddress shipmentAwareAddress)
                    {
                        shipmentAwareAddress.Shipment = this;
                    }
                    // Changing the shipping address will leading to change the sales tax of containing line items.
                    ResetUpToDateFlags(true);
                }
            }
        }

        public string ShipmentTrackingNumber { get; set; }

        public OrderShipmentStatus OrderShipmentStatus { get; set; }

        public int? PickListId { get; set; }

        public string WarehouseCode { get; set; }

        public ICollection<ILineItem> LineItems { get; set; }

        [JsonConverter(typeof(HashTableJsonConverter))]
        public Hashtable Properties { get; set; }

        public decimal ShipmentDiscount
        {
            get { return _shipmentDiscount; }
            set
            {
                if (_shipmentDiscount != value)
                {
                    _shipmentDiscount = value;
                    ResetUpToDateFlags();
                }
            }
        }

        public bool IsShippingCostUpToDate { get; set; }

        public bool IsShippingTaxUpToDate { get; set; }

        public decimal ShippingCost { get; set; }

        public decimal ShippingTax { get; set; }

        public IDictionary<int, IEnumerable<string>> OperationKeys
        {
            get
            {
                return _operationKeys;
            }
            set
            {
                // need setting for serialization.
                _operationKeys = value != null ? 
                      new ConcurrentDictionary<int, IEnumerable<string>>(value)
                    : new ConcurrentDictionary<int, IEnumerable<string>>();
            }
        }

        public bool ContainsOperationKeyFor(ILineItem lineItem)
        {
            var index = GetLineItemIndex(lineItem);
            return _operationKeys.ContainsKey(index);
        }

        public IEnumerable<string> GetOperationKeys(ILineItem lineItem)
        {
            var index = GetLineItemIndex(lineItem);
            return _operationKeys.TryGetValue(index, out var keys) ? keys : Enumerable.Empty<string>();
        }

        public void AddOperationKeys(ILineItem lineItem, IEnumerable<string> operationKeys)
        {
            var index = GetLineItemIndex(lineItem);
            if (index < 0)
            {
                return;
            }

            _operationKeys.AddOrUpdate(index, operationKeys, (k, v) => v);
        }

        public bool InsertOperationKeys(ILineItem lineItem, IEnumerable<string> operationKeys)
        {
            var index = GetLineItemIndex(lineItem);
            if (index < 0 || !_operationKeys.ContainsKey(index))
            {
                return false;
            }

            var existing = _operationKeys[index];
            _operationKeys[index] = existing.Union(operationKeys);
            return true;
        }

        public bool RemoveOperationKey(ILineItem lineItem, string operationKey)
        {
            var index = GetLineItemIndex(lineItem);
            if (index < 0 || !_operationKeys.ContainsKey(index))
            {
                return false;
            }

            var operationKeys = _operationKeys[index];
            if (operationKeys.Contains(operationKey))
            {
                _operationKeys[index] = operationKeys.Except(new List<string>() { operationKey });
                return true;
            }

            return false;
        }

        public void ClearOperationKeys()
        {
            _operationKeys.Clear();
        }

        int GetLineItemIndex(ILineItem lineItem)
        {
            var itemsList = LineItems.ToList();
            return itemsList.IndexOf(itemsList.FirstOrDefault(x => x.LineItemId == lineItem.LineItemId));
        }

        bool _deserialized;
        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            _deserialized = true;
        }

        internal void SetParent(CloudyOrderGroupBase newParent, bool keepIds)
        {
            if (newParent != null && newParent != _parent)
            {
                foreach (var lineItem in LineItems.OfType<CloudyLineItem>())
                {
                    lineItem.SetParent(newParent, keepIds);
                }
                if (!keepIds)
                {
                    var forms = new List<IOrderForm>(newParent.Forms);

                    if (newParent is IPurchaseOrder order)
                    {
                        forms.AddRange(order.ReturnForms);
                    }

                    ShipmentId = forms.SelectMany(x => x.Shipments).Select(y => y.ShipmentId).Max() + 1;
                }
            }
            _parent = newParent;
        }

        void LineItemsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            foreach (var newItem in args.GetNewItems<CloudyLineItem>())
            {
                newItem.SetParent(_parent, false);
            }

            ResetUpToDateFlags();
        }

        void ResetUpToDateFlags(bool includedLineItemSalesTax = false)
        {
            if (!_deserialized) return;

            IsShippingCostUpToDate = false;
            IsShippingTaxUpToDate = false;

            if (_parent != null)
            {
                _parent.IsTaxTotalUpToDate = false;
            }
            
            if (includedLineItemSalesTax)
            {
                foreach (var lineItem in LineItems.OfType<ILineItemCalculatedAmount>())
                {
                    lineItem.IsSalesTaxUpToDate = false;
                }
            }
        }
    }
}