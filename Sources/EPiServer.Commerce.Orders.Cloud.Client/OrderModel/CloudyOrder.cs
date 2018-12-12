using EPiServer.Commerce.Order;
using Mediachase.Commerce.Orders.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace EPiServer.Commerce.Orders.Cloud.Client.OrderModel
{
    [Serializable]
    public class CloudyOrder : CloudyOrderGroupBase, IPurchaseOrder, IDeepCloneable
    {
        public CloudyOrder()
        {
            var observableOrderFormCollection = new ObservableCollection<IReturnOrderForm>();
            observableOrderFormCollection.CollectionChanged += OrderFormsChanged;
            ReturnForms = observableOrderFormCollection;
        }

        [JsonConstructor]
        public CloudyOrder(ICollection<CloudyReturnOrderForm> returnForms, ICollection<CloudyOrderForm> forms, ICollection<CloudyOrderNote> notes) : base(forms, notes)
        {
            var observableOrderFormCollection = new ObservableCollection<IReturnOrderForm>(returnForms);
            observableOrderFormCollection.CollectionChanged += OrderFormsChanged;
            ReturnForms = observableOrderFormCollection;
        }

        public object DeepClone()
        {
            var newPurchaseOrderData = JsonConvert.SerializeObject(this);
            var clonedPurcahseOrder = JsonConvert.DeserializeObject<CloudyOrder>(newPurchaseOrderData);
            return clonedPurcahseOrder;
        }

        [Required]
        public string OrderNumber { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public ICollection<IReturnOrderForm> ReturnForms { get; set; }
    }
}
