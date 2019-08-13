using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.ViewModels
{
    public class LinkItemViewModel
    {
        public string Text { get; set; }
        public string Href { get; set; }
        public string Target { get; set; }
        public string Title { get; set; }
    }
}