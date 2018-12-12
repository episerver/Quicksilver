using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Internal.Serialization;
using EPiServer.Commerce.Serialization.Json;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using EPiServer.Commerce.Order.Internal;
using System.ComponentModel.DataAnnotations;

namespace EPiServer.Commerce.Orders.Cloud.Client.OrderModel
{

    public abstract class CloudyOrderGroupBase : IOrderGroup, IOrderGroupCalculatedAmount
    {
        IMarket _market;
        Currency _currency;
        MarketId _marketId;

        protected CloudyOrderGroupBase()
        {
            Created = DateTime.UtcNow;
            Modified = DateTime.UtcNow;

            var observableOrderFormCollection = new ObservableCollection<IOrderForm>();
            observableOrderFormCollection.CollectionChanged += OrderFormsChanged;
            Forms = observableOrderFormCollection;

            var observableOrderNoteCollection = new ObservableCollection<IOrderNote>();
            observableOrderNoteCollection.CollectionChanged += OrderNotesChanged;
            Notes = observableOrderNoteCollection;

            Properties = new Hashtable();
            OrderStatus = OrderStatus.InProgress;
        }

        [JsonConstructor]
        protected CloudyOrderGroupBase(ICollection<CloudyOrderForm> forms, ICollection<CloudyOrderNote> notes)
        {
            foreach (var form in forms)
            {
                form.SetParent(this, true);
            }

            foreach (var note in notes)
            {
                note.SetParentOrder(this, true);
            }

            var observableOrderFormCollection = new ObservableCollection<IOrderForm>(forms);
            observableOrderFormCollection.CollectionChanged += OrderFormsChanged;
            Forms = observableOrderFormCollection;

            var observableOrderNoteCollection = new ObservableCollection<IOrderNote>(notes);
            observableOrderNoteCollection.CollectionChanged += OrderNotesChanged;
            Notes = observableOrderNoteCollection;
        }

        public int OrderGroupId { get; set; }

        public DateTime Created { get; set; }

        [JsonConverter(typeof(CurrencyConverter))]
        public Currency Currency
        {
            get { return _currency; }
            set
            {
                if (_currency != value)
                {
                    _currency = value;
                    ResetUpToDateFlags();
                }
            }
        }

        public Guid CustomerId { get; set; }

        public ICollection<IOrderForm> Forms { get; set; }

        [JsonConverter(typeof(MarketConverter))]
        public IMarket Market
        {
            get { return _market; }
            set
            {
                if (_market != value)
                {
                    _market = value;
                    ResetUpToDateFlags();
                }
            }
        }

        [JsonProperty]
        public DateTime? Modified { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<IOrderNote> Notes { get; set; }

        public OrderReference OrderLink => new OrderReference(OrderGroupId, Name, CustomerId, GetType());

        public OrderStatus OrderStatus { get; set; }

        public Guid? Organization { get; set; }

        [JsonConverter(typeof(HashTableJsonConverter))]
        public Hashtable Properties { get; set; }

        public bool IsTaxTotalUpToDate { get; set; }

        public decimal TaxTotal { get; set; }

        [Required]
        [JsonConverter(typeof(MarketIdConverter))]
        public MarketId MarketId
        {
            get { return _marketId; }
            set
            {
                if (_marketId != value)
                {
                    _marketId = value;
                    ResetUpToDateFlags();
                }
            }
        }

        public string MarketName { get; set; }

        public bool PricesIncludeTax { get; set; }

        bool _deserialized;
        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            _deserialized = true;
        }

        protected void OrderFormsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            foreach (var newItem in args.GetNewItems<CloudyOrderForm>())
            {
               newItem.SetParent(this, false);
            }
        }

        void OrderNotesChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            foreach (var newItem in args.GetNewItems<CloudyOrderNote>())
            {
               newItem.SetParentOrder(this, false);
            }
        }

        void ResetUpToDateFlags()
        {
            if (!_deserialized) return;

            IsTaxTotalUpToDate = false;

            foreach (var shipmentCalculatedPrice in Forms.SelectMany(x => x.Shipments.OfType<IShipmentCalculatedAmount>()))
            {
                shipmentCalculatedPrice.ResetUpToDateFlags();
            }
        }
    }
}
