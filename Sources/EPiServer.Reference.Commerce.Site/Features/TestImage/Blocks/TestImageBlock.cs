using EPiServer.Core;
using EPiServer.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.TestImage.Blocks
{
    [ContentType(DisplayName = "Test Image Block",
        GUID = "32782B29-278B-410A-A402-9FF46FAF32B8",
        Description = "This is test only")]
    public class TestImageBlock : BlockData
    {
    }
}