using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace EPiServer.Commerce.OrderStore.Model
{
    public class OrderFormModel
    {
        public decimal AuthorizedPaymentTotal { get; set; }

        public decimal CapturedPaymentTotal { get; set; }

        public decimal HandlingTotal { get; set; }

        public string Name { get; set; }

        public List<ShipmentModel> Shipments { get; set; }

        public List<PaymentModel> Payments { get; set; }

        [JsonConverter(typeof(HashTableJsonConverter))]
        public Hashtable Properties { get; set; }

        public int OrderFormId { get; set; }

        public List<string> CouponCodes { get; set; }

        public List<PromotionInformationModel> Promotions { get; set; }

        public bool PricesIncludeTax { get; set; }


    }
}