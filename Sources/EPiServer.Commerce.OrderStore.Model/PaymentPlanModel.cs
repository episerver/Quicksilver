using System;
using System.ComponentModel.DataAnnotations;

namespace EPiServer.Commerce.OrderStore.Model
{
    public class PaymentPlanModel : OrderGroupModel
    {
        [EnumDataType(typeof(PaymentPlanCycleModel))]
        public PaymentPlanCycleModel CycleMode { get; set; }

        public int CycleLength { get; set; }

        public int MaxCyclesCount { get; set; }

        public int CompletedCyclesCount { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? LastTransactionDate { get; set; }

        public bool IsActive { get; set; }
    }
}
