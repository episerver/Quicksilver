using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Pricing;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Services
{
    [ServiceConfiguration(typeof(IPromotionService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class PromotionService : IPromotionService
    {
        private readonly IContentLoader _contentLoader;
        private readonly IPricingService _pricingService;
        private readonly IMarketService _marketService;
        private readonly ReferenceConverter _referenceConverter;
        private readonly ILineItemCalculator _lineItemCalculator;
        private readonly IPromotionEngine _promotionEngine;

        public PromotionService(
            IPricingService pricingService, 
            IMarketService marketService, 
            IContentLoader contentLoader, 
            ReferenceConverter referenceConverter,
            ILineItemCalculator lineItemCalculator,
            IPromotionEngine promotionEngine)
        {
            _contentLoader = contentLoader;
            _marketService = marketService;
            _pricingService = pricingService;
            _referenceConverter = referenceConverter;
            _lineItemCalculator = lineItemCalculator;
            _promotionEngine = promotionEngine;
        }
        
        public IList<IPriceValue> GetDiscountPriceList(IEnumerable<CatalogKey> catalogKeys, MarketId marketId, Currency currency)
        {
            var market = _marketService.GetMarket(marketId);
            if (market == null)
            {
                throw new ArgumentException(string.Format("market '{0}' does not exist", marketId));
            }

            var priceFilter = new PriceFilter
            {
                CustomerPricing = new[] { CustomerPricing.AllCustomers },
                Quantity = 1,
                ReturnCustomerPricing = true,
            };
            if (currency != Currency.Empty)
            {
                priceFilter.Currencies = new[] { currency };
            }
            var prices = catalogKeys.SelectMany(x => _pricingService.GetPriceList(x.CatalogEntryCode, marketId, priceFilter));

            return GetDiscountPrices(prices.ToList(), market, currency);
        }

        public IPriceValue GetDiscountPrice(CatalogKey catalogKey, MarketId marketId, Currency currency)
        {
            return GetDiscountPriceList(new[] { catalogKey }, marketId, currency).FirstOrDefault();
        }


        public IPriceValue GetDiscountPrice(IPriceValue price, EntryContentBase entry, Currency currency, IMarket market)
        {
            var discountedPrice = _promotionEngine.GetDiscountPrices(new[] { entry.ContentLink }, market, currency, _referenceConverter, _lineItemCalculator);
            if (discountedPrice.Any())
            {
                var highestDiscount = discountedPrice.SelectMany(x => x.DiscountPrices).OrderBy(x => x.Price).FirstOrDefault().Price;
                return new PriceValue
                {
                    CatalogKey = price.CatalogKey,
                    CustomerPricing = CustomerPricing.AllCustomers,
                    MarketId = price.MarketId,
                    MinQuantity = 1,
                    UnitPrice = highestDiscount,
                    ValidFrom = DateTime.UtcNow,
                    ValidUntil = null
                };
            }
            return price;
        }

        private IList<IPriceValue> GetDiscountPrices(IList<IPriceValue> prices, IMarket market, Currency currency)
        {
            currency = GetCurrency(currency, market);
            var priceValues = new List<IPriceValue>();
            
            foreach (var entry in GetEntries(prices))
            {
                var price = prices
                    .OrderBy(x => x.UnitPrice.Amount)
                    .FirstOrDefault(x => x.CatalogKey.CatalogEntryCode.Equals(entry.Code) &&
                        x.UnitPrice.Currency.Equals(currency));
                if (price == null)
                {
                    continue;
                }

                priceValues.Add(GetDiscountPrice(
                    price, entry, currency, market));
                
            }
            return priceValues;
        }

        private Currency GetCurrency(Currency currency, IMarket market)
        {
            return currency == Currency.Empty ? market.DefaultCurrency : currency;
        }

        private IEnumerable<EntryContentBase> GetEntries(IEnumerable<IPriceValue> prices)
        {
            return prices.GroupBy(x => x.CatalogKey.CatalogEntryCode)
                .Select(x => x.First())
                .Select(x => _contentLoader.Get<EntryContentBase>(
                    _referenceConverter.GetContentLink(x.CatalogKey.CatalogEntryCode, CatalogContentType.CatalogEntry)));
        }
    }
}