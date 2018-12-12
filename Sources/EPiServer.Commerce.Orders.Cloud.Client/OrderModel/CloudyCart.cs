using EPiServer.Commerce.Order;
using Mediachase.Commerce.Orders.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EPiServer.Commerce.Orders.Cloud.Client.OrderModel
{
    [Serializable]
    public class CloudyCart : CloudyOrderGroupBase, ICart, IDeepCloneable
    {
        public CloudyCart()
        {
        }
        
        [JsonConstructor]
        public CloudyCart(ICollection<CloudyOrderForm> forms, ICollection<CloudyOrderNote> notes) : base(forms, notes)
        {
            
        }
        
        public object DeepClone()
        {
            var newCartData = JsonConvert.SerializeObject(this);
            var clonedCart = JsonConvert.DeserializeObject<CloudyCart>(newCartData);
            return clonedCart;
        }
    }
}
