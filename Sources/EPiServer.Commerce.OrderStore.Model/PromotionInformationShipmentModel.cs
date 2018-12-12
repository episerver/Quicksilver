using System;

namespace EPiServer.Commerce.OrderStore.Model
{
    public class PromotionInformationShipmentModel
    {

        public Guid ShippingMethodId { get; set; }

        public string OrderAddressName { get; set; }

        public string ShippingMethodName { get; set; }

        public decimal SavedAmount { get; set; }
    }
}