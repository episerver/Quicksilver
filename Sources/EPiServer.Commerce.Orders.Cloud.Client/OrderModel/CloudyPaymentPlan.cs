using EPiServer.Commerce.Order;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EPiServer.Commerce.Orders.Cloud.Client.OrderModel
{
    [Serializable]
    public class CloudyPaymentPlan : CloudyOrderGroupBase, IPaymentPlan, IDeepCloneable
    {
        public CloudyPaymentPlan()
        {
        }

        [JsonConstructor]
        public CloudyPaymentPlan(ICollection<CloudyOrderForm> forms, ICollection<CloudyOrderNote> notes) : base(forms, notes)
        {

        }

        public object DeepClone()
        {
            var newPaymentPlanData = JsonConvert.SerializeObject(this);
            var clonedPaymentPlan = JsonConvert.DeserializeObject<CloudyPaymentPlan>(newPaymentPlanData);
            return clonedPaymentPlan;
        }

        public PaymentPlanCycle CycleMode { get; set; }
        public int CycleLength { get; set; }
        public int MaxCyclesCount { get; set; }
        public int CompletedCyclesCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? LastTransactionDate { get; set; }
        public bool IsActive { get; set; }
    }
}
