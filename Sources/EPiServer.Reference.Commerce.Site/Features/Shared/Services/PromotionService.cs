using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Marketing;
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
        private readonly IPromotionEntryService _promotionEntryService;
        private readonly PromotionHelperFacade _promotionHelper;

        public PromotionService(
            IPricingService pricingService, 
            IMarketService marketService, 
            IContentLoader contentLoader, 
            ReferenceConverter referenceConverter, 
            PromotionHelperFacade promotionHelper,
            IPromotionEntryService promotionEntryService)
        {
            _contentLoader = contentLoader;
            _marketService = marketService;
            _pricingService = pricingService;
            _referenceConverter = referenceConverter;
            _promotionEntryService = promotionEntryService;
            _promotionHelper = promotionHelper;
        }
        
        public IList<IPriceValue> GetDiscountPriceList(IEnumerable<CatalogKey> catalogKeys, MarketId marketId, Currency currency)
        {
            if (_marketService.GetMarket(marketId) == null)
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

            return GetDiscountPrices(prices.ToList(), marketId, currency);
        }

        public IPriceValue GetDiscountPrice(CatalogKey catalogKey, MarketId marketId, Currency currency)
        {
            return GetDiscountPriceList(new[] { catalogKey }, marketId, currency).FirstOrDefault();
        }

        private IList<IPriceValue> GetDiscountPrices(IList<IPriceValue> prices, MarketId marketId, Currency currency)
        {
            currency = GetCurrency(currency, marketId);

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

                priceValues.Add(_promotionEntryService.GetDiscountPrice(
                    price, entry, currency, _promotionHelper));
                
            }
            return priceValues;
        }
       
        private Currency GetCurrency(Currency currency, MarketId marketId)
        {
            return currency == Currency.Empty ? _marketService.GetMarket(marketId).DefaultCurrency : currency;
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