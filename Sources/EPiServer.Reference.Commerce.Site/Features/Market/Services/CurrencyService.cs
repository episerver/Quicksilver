using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Market.Services
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
            Currency currency;
            return TryGetCurrency(_cookieService.Get(CurrencyCookie), out currency) ? 
                currency : 
                CurrentMarket.DefaultCurrency;
        }

        public bool SetCurrentCurrency(string currencyCode)
        {
            Currency currency;
            
            if (!TryGetCurrency(currencyCode, out currency))
            {
                return false;
            }
                
            _cookieService.Set(CurrencyCookie, currencyCode);

            return true;
        }

        private bool TryGetCurrency(string currencyCode, out Currency currency)
        {
            var result = GetAvailableCurrencies()
                .Where(x => x.CurrencyCode == currencyCode)
                .Cast<Currency?>()
                .FirstOrDefault();

            if (result.HasValue)
            {
                currency = result.Value;
                return true;
            }

            currency = null;
            return false;
        }

        private IMarket CurrentMarket => _currentMarket.GetCurrentMarket();
    }
}