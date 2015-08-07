using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.Folder.Editorial.Blocks
{
    [ContentType(DisplayName = "Free text block", GUID = "32782B29-278B-410A-A402-9FF46FAF32B9", Description = "")]
    public class FreeTextBlock : BlockData
    {
        [CultureSpecific]
        [Display(
            Name = "Main body",
            Description = "",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual XhtmlString MainBody { get; set; }
    }
}