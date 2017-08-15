using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using Mediachase.Commerce.Markets;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Services
{
    [ServiceConfiguration(typeof(IPricingService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class PricingService : IPricingService
    {
        private readonly IPriceService _priceService;
        private readonly ICurrentMarket _currentMarket;
        private readonly ICurrencyService _currencyService;
        private readonly CatalogContentService _catalogContentService;
        private readonly ReferenceConverter _referenceConverter;
        private readonly IMarketService _marketService;
        private readonly ILineItemCalculator _lineItemCalculator;
        private readonly IPromotionEngine _promotionEngine;

        public PricingService(
            IPriceService priceService,
            ICurrentMarket currentMarket, 
            ICurrencyService currencyService,
            CatalogContentService catalogContentService, 
            ReferenceConverter referenceConverter, 
            IMarketService marketService, 
            ILineItemCalculator lineItemCalculator, 
            IPromotionEngine promotionEngine)
        {
            _priceService = priceService;
            _currentMarket = currentMarket;
            _currencyService = currencyService;
            _catalogContentService = catalogContentService;
            _referenceConverter = referenceConverter;
            _marketService = marketService;
            _lineItemCalculator = lineItemCalculator;
            _promotionEngine = promotionEngine;
        }

        public IPriceValue GetPrice(string code)
        {
            return GetPrice(code, _currentMarket.GetCurrentMarket().MarketId, _currencyService.GetCurrentCurrency());
        }

        public IPriceValue GetPrice(string code, MarketId marketId, Currency currency)
        {
            return _priceService.GetPrices(
                marketId, 
                DateTime.Now, 
                new CatalogKey(code), 
                new PriceFilter { Currencies = new[] { currency } })
                .OrderBy(x => x.UnitPrice.Amount).FirstOrDefault();
        }

        public IPriceValue GetDefaultPrice(string code)
        {
            return _priceService.GetDefaultPrice(_currentMarket.GetCurrentMarket().MarketId, DateTime.Now, new CatalogKey(code), _currencyService.GetCurrentCurrency());
        }

        public IEnumerable<IPriceValue> GetCatalogEntryPrices(IEnumerable<CatalogKey> catalogKeys)
        {
            return _priceService.GetCatalogEntryPrices(catalogKeys);
        }

        public IPriceValue GetDiscountPrice(string code)
        {
            return GetDiscountPrice(new CatalogKey(code), _currentMarket.GetCurrentMarket().MarketId, _currencyService.GetCurrentCurrency());
        }

        public IPriceValue GetDiscountPrice(CatalogKey catalogKey, MarketId marketId, Currency currency)
        {
            var market = _marketService.GetMarket(marketId);

            currency = currency != Currency.Empty ? currency : market.DefaultCurrency;

            var priceFilter = new PriceFilter
            {
                CustomerPricing = new[] { CustomerPricing.AllCustomers },
                Quantity = 1,
                ReturnCustomerPricing = true,
                Currencies = new[] { currency } 
            };
           
            var prices = _priceService.GetPrices(marketId, DateTime.Now, catalogKey, priceFilter)
                .OrderBy(x => x.UnitPrice.Amount)
                .ToList();

            foreach (var entry in GetEntries(prices))
            {
                var price = prices
                    .FirstOrDefault(x => x.CatalogKey.CatalogEntryCode.Equals(entry.Code) && x.UnitPrice.Currency.Equals(currency));
                if (price == null)
                {
                    continue;
                }

                var discountPrices = GetDiscountedPrices(entry.ContentLink, market, currency);
                if (!discountPrices.Any())
                {
                    return price;
                }

                return new PriceValue
                {
                    CatalogKey = price.CatalogKey,
                    CustomerPricing = CustomerPricing.AllCustomers,
                    MarketId = price.MarketId,
                    MinQuantity = 1,
                    UnitPrice = discountPrices.SelectMany(x => x.DiscountPrices).OrderBy(x => x.Price).First().Price,
                    ValidFrom = DateTime.UtcNow,
                    ValidUntil = null
                };
            }

            return null;
        }

        public Money GetMoney(decimal amount)
        {
            return new Money(amount, _currencyService.GetCurrentCurrency());
        }

        public Money GetDiscountedPrice(ILineItem lineItem, Currency currency)
        {
            return lineItem.GetDiscountedPrice(currency, _lineItemCalculator);
        }

        protected virtual IEnumerable<EntryContentBase> GetEntries(IEnumerable<IPriceValue> prices)
        {
            return prices.GroupBy(x => x.CatalogKey.CatalogEntryCode)
                .Select(x => x.First())
                .Select(x => _catalogContentService.Get<EntryContentBase>(x.CatalogKey.CatalogEntryCode));
        }

        protected virtual IEnumerable<DiscountedEntry> GetDiscountedPrices(ContentReference contentLink, IMarket market, Currency currency)
        {
            return _promotionEngine.GetDiscountPrices(new[] { contentLink }, market, currency, _referenceConverter, _lineItemCalculator);
        }
    }
}