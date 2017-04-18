using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.ViewModels;
using Mediachase.Commerce;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Product.ViewModels
{
    public class FashionPackageViewModel : ProductViewModelBase
    {
        public FashionPackage Package { get; set; }
        public Money? DiscountedPrice { get; set; }
        public Money ListingPrice { get; set; }
        public IEnumerable<CatalogContentBase> Entries { get; set; }
        public bool IsAvailable { get; set; }
    }
}