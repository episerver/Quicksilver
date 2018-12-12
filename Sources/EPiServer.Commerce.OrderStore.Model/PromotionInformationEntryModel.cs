using System.ComponentModel.DataAnnotations;

namespace EPiServer.Commerce.OrderStore.Model
{
    public class PromotionInformationEntryModel
    {
        public string EntryCode { get; set; }

        public decimal SavedAmount { get; set; }
    }
}