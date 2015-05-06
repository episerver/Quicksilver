using System.Collections.Generic;
using EPiServer.Reference.Commerce.Site.Features.Product.Blocks;

namespace EPiServer.Reference.Commerce.Site.Features.Product.Models
{
    public class ProductListBlockViewModel
    {
        public ProductListBlock CurrentBlock { get; set; }
        public IEnumerable<ProductViewModel> Products { get; set; }
    }
}