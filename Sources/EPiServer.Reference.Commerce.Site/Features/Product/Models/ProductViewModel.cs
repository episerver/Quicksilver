using EPiServer.Core;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Features.Product.Models
{
    public class ProductViewModel
    {
        public string DisplayName { get; set; }
        public string Image { get; set; }
        public string Url { get; set; }
        public Money Price { get; set; }
        public Money OriginalPrice { get; set; }
        public string Code { get; set; }
        public bool IsWishList { get; set; }
        public ContentReference ContentLink { get; set; }
    }
}