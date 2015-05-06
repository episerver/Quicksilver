using EPiServer.Commerce;
using EPiServer.Commerce.SpecializedProperties;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Framework.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.Media.Models
{
    [ContentType(GUID = "20644be7-3ca1-4f84-b893-ee021b73ce6c")]
    [MediaDescriptor(ExtensionString = "jpg,jpeg,jpe,ico,gif,bmp,png")]
    public class ImageMediaData : CommerceImage
    {
        [CultureSpecific]
        [Display(
            Name= "Alternate text",
            Description= "Description of the image",
            GroupName = SystemTabNames.Content,
            Order = 10)]
        public virtual string Description { get; set; }

        [CultureSpecific]
        [Display(
            Name = "Link",
            Description = "Link to content",
            GroupName = SystemTabNames.Content,
            Order = 20)]
        [UIHint(UIHint.AllContent)]
        public virtual ContentReference Link { get; set; }
    }
}