using EPiServer.Commerce.Order;
using Mediachase.Commerce.Orders;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Commerce.Orders.Cloud.Client.OrderModel
{
    public class CloudyReturnOrderForm : CloudyOrderForm, IReturnOrderForm
    {
        public CloudyReturnOrderForm() : base()
        {}

        [JsonConstructor]
        public CloudyReturnOrderForm(ICollection<CloudyShipment> shipments, ICollection<CloudyPayment> payments) : base(shipments, payments)
        {}

        protected override int GenerateId(IOrderGroup parent)
        {
            var returnForms = ((IPurchaseOrder) parent).ReturnForms;
            return returnForms.Any() ? returnForms.Select(x => x.OrderFormId).Max() + 1 : 1;
        }

        public int? OriginalOrderFormId { get; set; }
        public int? ExchangeOrderGroupId { get; set; }
        public string ReturnAuthCode { get; set; }
        public string RMANumber { get; set; }
        public string ReturnType { get; set; }
        public string ReturnComment { get; set; }
        public ReturnFormStatus Status { get; set; }
    }
}
