using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Services
{
    [ServiceConfiguration(typeof(IPricingService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class PricingService : IPricingService
    {
        private readonly IPriceService _priceService;
        private readonly ICurrentMarket _currentMarket;
        private readonly ICurrencyService _currencyService;
        private AppContextFacade _appContext;

        public PricingService(IPriceService priceService,
            ICurrentMarket currentMarket, 
            ICurrencyService currencyService,
            AppContextFacade appContext)
        {
            _priceService = priceService;
            _currentMarket = currentMarket;
            _currencyService = currencyService;
            _appContext = appContext;
        }

        public IList<IPriceValue> GetPriceList(string code, MarketId marketId, PriceFilter priceFilter)
        {
            if (String.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }

            var catalogKey = new CatalogKey(_appContext.ApplicationId, code);

            return _priceService.GetPrices(marketId, DateTime.Now, catalogKey, priceFilter)
                .OrderBy(x => x.UnitPrice.Amount)
                .ToList();
        }

        public IList<IPriceValue> GetPriceList(IEnumerable<CatalogKey> catalogKeys, MarketId marketId, PriceFilter priceFilter)
        {
            if (catalogKeys == null)
            {
                throw new ArgumentNullException("catalogKeys");
            }

            if (!catalogKeys.Any())
            {
                return Enumerable.Empty<IPriceValue>().ToList();
            }

            return _priceService.GetPrices(marketId, DateTime.Now, catalogKeys, priceFilter)
                .OrderBy(x => x.UnitPrice.Amount)
                .ToList();
        }
        
        public Money GetCurrentPrice(string code)
        {
            var market = _currentMarket.GetCurrentMarket();
            var currency = _currencyService.GetCurrentCurrency();
            var prices = GetPriceList(code, market.MarketId,
                new PriceFilter
                {
                    Currencies = new[] { currency }
                });

            return prices.Any() ? prices.First().UnitPrice : new Money(0, currency);
        }
    }
}