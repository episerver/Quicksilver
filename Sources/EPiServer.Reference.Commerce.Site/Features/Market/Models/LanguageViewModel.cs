using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;
using EPiServer.Core;

namespace EPiServer.Reference.Commerce.Site.Features.Market.Models
{
    public class LanguageViewModel
    {
        public IEnumerable<SelectListItem> Languages { get; set; }
        public string Language { get; set; }
        public ContentReference ContentLink { get; set; }
    }
}