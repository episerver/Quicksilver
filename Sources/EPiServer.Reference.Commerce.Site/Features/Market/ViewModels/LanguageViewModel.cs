using System.Collections.Generic;
using System.Web.Mvc;
using EPiServer.Core;

namespace EPiServer.Reference.Commerce.Site.Features.Market.ViewModels
{
    public class LanguageViewModel
    {
        public IEnumerable<SelectListItem> Languages { get; set; }
        public string Language { get; set; }
        public ContentReference ContentLink { get; set; }
    }
}