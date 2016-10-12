using System.Collections.Generic;
using System.Web.Mvc;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Features.Product.ViewModels
{
    public class FashionProductViewModel
    {
        public FashionProduct Product { get; set; }
        public Money? DiscountedPrice { get; set; }
        public Money ListingPrice { get; set; }
        public FashionVariant Variation { get; set; }
        public IList<SelectListItem> Colors { get; set; }
        public IList<SelectListItem> Sizes { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public IList<string> Images { get; set; }
        public bool IsAvailable { get; set; }
    }
}