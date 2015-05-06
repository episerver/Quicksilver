using System.Collections.Generic;
using System.Web.Mvc;
using Mediachase.Commerce;
using Mediachase.Commerce.Pricing;

namespace EPiServer.Reference.Commerce.Site.Features.Product.Models
{
    public class FashionProductViewModel
    {
        public FashionProduct Product { get; set; }
        public Money Price { get; set; }
        public Money OriginalPrice { get; set; }
        public FashionVariant Variation { get; set; }

        public IEnumerable<SelectListItem> Colors { get; set; }
        public IEnumerable<SelectListItem> Sizes { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public IEnumerable<string> Images { get; set; }
    }
}