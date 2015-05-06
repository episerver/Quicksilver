using System.ComponentModel.DataAnnotations;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.Product.Blocks
{
    [ContentType(DisplayName = "ProductListBlock", GUID = "ba75739f-a827-4f21-9538-9fd33719c3e7", Description = "")]
    public class ProductListBlock : BlockData
    {
        [CultureSpecific]
        [Display(
            Name = "Display name",
            Description = "",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual string DisplayName { get; set; }

        [AllowedTypes(typeof(ProductContent))]
        [CultureSpecific]
        [Display(
            Name = "Products",
            Description = "",
            GroupName = SystemTabNames.Content,
            Order = 2)]
        public virtual ContentArea Products { get; set; }
    }
}