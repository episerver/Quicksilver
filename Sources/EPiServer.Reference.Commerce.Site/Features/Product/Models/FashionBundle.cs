using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.Product.Models
{
    [CatalogContentType(
        GUID = "F403ABFF-6C95-4B5B-AB7D-C15AE6055537",
        DisplayName = "Fashion Bundle",
        MetaClassName = "FashionBundle",
        Description = "Displays a bundle, which is collection of individual fashion variants.")]
    public class FashionBundle : BundleContent
    {
        [Searchable]
        [CultureSpecific]
        [Tokenize]
        [IncludeInDefaultSearch]
        [Display(Name = "Description", Order = 1)]
        public virtual XhtmlString Description { get; set; }
    }
}