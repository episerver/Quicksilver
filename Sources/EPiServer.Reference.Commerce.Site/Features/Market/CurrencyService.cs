using System.Collections.Generic;
using System.Linq;
using EPiServer.Reference.Commerce.Site.Infrastructure;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Features.Market
{
    [ServiceConfiguration(typeof(ICurrencyService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class CurrencyService : ICurrencyService
    {
        private const string CurrencyCookie = "Currency";
        private readonly ICurrentMarket _currentMarket;
        private readonly CookieService _cookieService;

        public CurrencyService(ICurrentMarket currentMarket, CookieService cookieService)
        {
            _currentMarket = currentMarket;
            _cookieService = cookieService;
        }

        public IEnumerable<Currency> GetAvailableCurrencies()
        {
            return CurrentMarket.Currencies;
        }

        public virtual Currency GetCurrentCurrency()
        {
            return TryGetCurrency(_cookieService.Get(CurrencyCookie)) ?? CurrentMarket.DefaultCurrency;
        }

        public bool SetCurrentCurrency(string currencyCode)
        {
            var currency = TryGetCurrency(currencyCode);
            if (currency == null)
            {
                return false;
            }
                
            _cookieService.Set(CurrencyCookie, currencyCode);
            return true;
        }

        private Currency? TryGetCurrency(string currencyCode)
        {
            return GetAvailableCurrencies()
                .Where(x => x.CurrencyCode == currencyCode)
                .Cast<Currency?>()
                .FirstOrDefault();
        }

        private IMarket CurrentMarket
        {
            get { return _currentMarket.GetCurrentMarket(); }
        }
    }
}