using System.Collections.Generic;
using System.Web.Mvc;
using EPiServer.Core;

namespace EPiServer.Reference.Commerce.Site.Features.Market.ViewModels
{
    public class MarketViewModel
    {
        public IEnumerable<SelectListItem> Markets { get; set; }
        public string MarketId { get; set; }
        public ContentReference ContentLink { get; set; }
    }
}