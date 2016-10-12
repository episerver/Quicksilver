using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using Mediachase.Commerce;
using Mediachase.Commerce.Core;
using System;

namespace EPiServer.Reference.Commerce.Site.Infrastructure
{
    /// <summary>
    /// The default site context will resolve the currency to be the default currency for the current market.
    /// This site context makes sure the the Commerce API's uses the same currency that the user selects in the currency selector.
    /// This replaces the ordinary DefaultSiteContext by registering it in the container in <see cref="SiteInitialization"/>.
    /// </summary>
    public class CustomCurrencySiteContext : DefaultSiteContext
    {
        private Lazy<Currency> _lazyCurrency;

        public CustomCurrencySiteContext(ICurrencyService currencyService, ICurrentMarket currentMarket) 
            : base(currentMarket)
        {
            _lazyCurrency = new Lazy<Currency>(() => currencyService.GetCurrentCurrency());
        }

        public override Currency Currency
        {
            get { return _lazyCurrency.Value; }
            set { _lazyCurrency = new Lazy<Currency>(() => value); }
        }
    }
}