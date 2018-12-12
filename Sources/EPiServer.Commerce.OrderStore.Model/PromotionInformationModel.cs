using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EPiServer.Commerce.OrderStore.Model
{
    public class PromotionInformationModel
    {
        public Guid PromotionGuid { get; set; }

        public Guid? VisitorGroup { get; set; }

        public string CouponCode { get; set; }

        public string AdditionalInformation { get; set; }

        public int OrderFormId { get; set; }

        public PromotionInformationOrderFormModel OrderForm { get; set; }

        public List<PromotionInformationShipmentModel> Shipments { get; set; }

        public List<PromotionInformationEntryModel> Entries { get; set; }

        public bool IsRedeemed { get; set; }

        [EnumDataType(typeof(DiscountTypeModel))]
        public DiscountTypeModel DiscountType { get; set; }

        [EnumDataType(typeof(RewardTypeModel))]
        public RewardTypeModel RewardType { get; set; }

        public decimal SavedAmount { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        public Guid CustomerId { get; set; }
    }
}