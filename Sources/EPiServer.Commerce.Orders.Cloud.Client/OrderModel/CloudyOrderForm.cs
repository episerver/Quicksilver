using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Serialization.Json;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace EPiServer.Commerce.Orders.Cloud.Client.OrderModel
{
    [Serializable]
    public class CloudyOrderForm : IOrderForm
    {
        CloudyOrderGroupBase _parent;
  
        public CloudyOrderForm()
        {
            var observableShipmentCollection = new ObservableCollection<IShipment>();
            observableShipmentCollection.CollectionChanged += ShipmentsChanged;
            Shipments = observableShipmentCollection;
            CouponCodes = new List<string>();
            Payments = new List<IPayment>();
            Promotions = new List<PromotionInformation>();
            Properties = new Hashtable();

            AuthorizedPaymentTotal = 0;
            CapturedPaymentTotal = 0;
            HandlingTotal = 0;
        }

        [JsonConstructor]
        public CloudyOrderForm(ICollection<CloudyShipment> shipments, ICollection<CloudyPayment> payments)
        {
            foreach (var shipment in shipments)
            {
                shipment.SetParent(null, true);
            }

            var observableShipmentCollection = new ObservableCollection<IShipment>(shipments);
            observableShipmentCollection.CollectionChanged += ShipmentsChanged;
            Shipments = observableShipmentCollection;

            Payments = payments.Cast<IPayment>().ToList();
            Promotions = new List<PromotionInformation>();
            CouponCodes = new List<string>();
        }


        public int OrderFormId { get; set; }

        public decimal AuthorizedPaymentTotal { get; set; }

        public decimal CapturedPaymentTotal { get; set; }

        public decimal HandlingTotal { get; set; }

        public string Name { get; set; }

        public ICollection<IShipment> Shipments { get; set; }

        public IList<PromotionInformation> Promotions { get; }

        public ICollection<string> CouponCodes { get; set; }

        public ICollection<IPayment> Payments { get; set; }

        [JsonConverter(typeof(HashTableJsonConverter))]
        public Hashtable Properties { get; set; }

        public bool PricesIncludeTax { get; set; }

        internal void SetParent(CloudyOrderGroupBase newParent, bool keepIds)
        {
            if (newParent != _parent)
            {
                foreach (var shipment in Shipments.OfType<CloudyShipment>())
                {
                    shipment.SetParent(newParent, keepIds);
                }

                if (!keepIds)
                {
                    OrderFormId = GenerateId(newParent);
                }
            }
            _parent = newParent;
        }

        protected virtual int GenerateId(IOrderGroup parent)
        {
            return parent.Forms.Select(x => x.OrderFormId).Max() + 1;
        }

        void ShipmentsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            foreach (var newItem in args.GetNewItems<CloudyShipment>())
            {
                newItem.SetParent(_parent, false);
            }
        }
    }
}