using System.Collections.Generic;
using EPiServer.Core;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Features.Market.Models
{
    public class MarketViewModel
    {
        public IEnumerable<IMarket> Markets { get; set; }
        public IMarket CurrentMarket { get; set; }
        public ContentReference ContentLink { get; set; }
    }
}