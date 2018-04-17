using Mediachase.Commerce;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Market.Models
{
    public class EmptyMarket : IMarket
    {
        public IEnumerable<string> Countries => Enumerable.Empty<string>();

        public IEnumerable<Currency> Currencies => Enumerable.Empty<Currency>();

        public Currency DefaultCurrency => Currency.USD;

        public CultureInfo DefaultLanguage => CultureInfo.CurrentUICulture;

        public bool IsEnabled => true;
        
        public bool PricesIncludeTax => false;


        public IEnumerable<CultureInfo> Languages => Enumerable.Empty<CultureInfo>();

        public string MarketDescription => string.Empty;

        public MarketId MarketId => new MarketId("US");

        public string MarketName => string.Empty;
    }
}