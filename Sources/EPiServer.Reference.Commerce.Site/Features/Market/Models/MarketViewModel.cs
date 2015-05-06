using System.Collections.Generic;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Features.Market.Models
{
    public class MarketViewModel
    {
        public IEnumerable<IMarket> Markets { get; set; }
        public IMarket CurrentMarket { get; set; }
    }
}