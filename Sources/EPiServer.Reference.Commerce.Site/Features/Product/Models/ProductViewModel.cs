using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Features.Product.Models
{
    public class ProductViewModel : IProductModel
    {
        public string DisplayName { get; set; }
        public string ImageUrl { get; set; }
        public string Url { get; set; }
        public string Brand { get; set; }
        public Money ExtendedPrice { get; set; }
        public Money PlacedPrice { get; set; }
        public string Code { get; set; }
    }
}