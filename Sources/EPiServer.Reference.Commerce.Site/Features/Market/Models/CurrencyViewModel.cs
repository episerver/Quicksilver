using System.Collections.Generic;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Features.Market.Models
{
    public class CurrencyViewModel
    {
        public IEnumerable<Currency> Currencies { get; set; }
        public Currency CurrentCurrency { get; set; }
    }
}