using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.Folder.Editorial.Blocks
{
    [ContentType(DisplayName = "Map block", GUID = "32782B29-278B-410A-A402-9FF46FAF3212", Description = "MapBlock data")]
    public class MapBlock : BlockData
    {
        [Display(
            Name = "Latitude",
            Description = "Latitude",
            GroupName = SystemTabNames.Content,
            Order = 1)]
        public virtual string Latitude { get; set; }
        [Display(
            Name = "Longitude",
            Description = "Longitude",
            GroupName = SystemTabNames.Content,
            Order = 2)]
        public virtual string Longitude { get; set; }
        [CultureSpecific]
        [Display(
            Name = "Description",
            Description = "Localtion description",
            GroupName = SystemTabNames.Content,
            Order = 3)]
        public virtual string Description { get; set; }
    }
}