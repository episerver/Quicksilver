using System.ComponentModel.DataAnnotations;

namespace EPiServer.Commerce.OrderStore.Model
{
    public class ReturnOrderFormModel : OrderFormModel
    {
        public int? OriginalOrderFormId { get; set; }
        public int? ExchangeOrderGroupId { get; set; }
        public string ReturnAuthCode { get; set; }
        public string RMANumber { get; set; }
        public string ReturnType { get; set; }
        public string ReturnComment { get; set; }
        [EnumDataType(typeof(ReturnFormStatusModel))]
        public ReturnFormStatusModel Status { get; set; }
    }
}
