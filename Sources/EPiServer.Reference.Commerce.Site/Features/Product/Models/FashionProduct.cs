using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.DataAnnotations;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.Core;
using System.ComponentModel.DataAnnotations;
using EPiServer.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.Product.Models
{
    [CatalogContentType(
        GUID = "550ebcfc-c989-4272-8f94-c6d079f56181", 
        MetaClassName = "FashionProduct", 
        DisplayName = "Fashion product", 
        Description = "Display fashion product")]
    public class FashionProduct : ProductContent
    {
        [Searchable]
        [CultureSpecific]
        [Tokenize]
        [IncludeInDefaultSearch]
        [BackingType(typeof(PropertyString))]
        [Display(Name = "Brand", Order = 1)]
        public virtual string Brand { get; set; }

        [Searchable]
        [CultureSpecific]
        [Tokenize]
        [IncludeInDefaultSearch]
        [Display(Name = "Description", Order = 2)]
        public virtual XhtmlString Description { get; set; }

        [Searchable]
        [CultureSpecific]
        [Tokenize]
        [IncludeInDefaultSearch]
        [Display(Name = "Long Description", Order = 3)]
        public virtual XhtmlString LongDescription { get; set; }

        [Searchable]
        [CultureSpecific]
        [Tokenize]
        [IncludeInDefaultSearch]
        [Display(Name = "Sizing", Order = 4)]
        public virtual XhtmlString Sizing { get; set; }

        [CultureSpecific]
        [Display(Name = "Product Teaser", Order = 5)]
        public virtual XhtmlString ProductTeaser { get; set; }

        [Searchable]
        [IncludeInDefaultSearch]
        [BackingType(typeof(PropertyDictionaryMultiple))]
        [Display(Name = "Available Sizes", Order = 6)]
        public virtual ItemCollection<string> AvailableSizes { get; set; }

        [Searchable]
        [IncludeInDefaultSearch]
        [BackingType(typeof(PropertyDictionaryMultiple))]
        [Display(Name = "Available Colors", Order = 6)]
        public virtual ItemCollection<string> AvailableColors { get; set; }
    }
}