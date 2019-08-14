using EPiServer.Core;
using EPiServer.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Test.Pages
{
    [ContentType(DisplayName = "Test page",
        GUID = "452d1812-7385-42c3-8073-c1b7481e7b21",
        Description = "")]
    public class TestPage : PageData
    {
        public virtual string PageTitle { get; set; }

        public virtual ContentArea MyBlocks { get; set; }
    }
}