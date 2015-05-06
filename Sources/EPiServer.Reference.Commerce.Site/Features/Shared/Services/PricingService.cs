using EPiServer.Reference.Commerce.Site.Features.Market;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Marketing;
using Mediachase.Commerce.Marketing.Objects;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Pricing;
using Mediachase.Commerce.Website.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Services
{
    [ServiceConfiguration(typeof(IPricingService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class PricingService : IPricingService
    {
        private readonly IPriceService _priceService;
        private readonly ICurrentMarket _currentMarket;
        private readonly CurrencyService _currencyService;
        private readonly ICatalogSystem _catalogSystem;
        private readonly IPromotionEntryPopulate _promotionEntryPopulate;
        private readonly IMarketService _marketService;

        public PricingService(IPriceService priceService,
            ICurrentMarket currentMarket, 
            CurrencyService currencyService,
            ICatalogSystem catalogSystem,
            IMarketService marketService)
        {
            _priceService = priceService;
            _currentMarket = currentMarket;
            _currencyService = currencyService;
            _promotionEntryPopulate = (IPromotionEntryPopulate)MarketingContext.Current.PromotionEntryPopulateFunctionClassInfo.CreateInstance();
            _catalogSystem = catalogSystem;
            _marketService = marketService;

        }

        public IList<IPriceValue> GetPriceList(string code, MarketId marketId, PriceFilter priceFilter)
        {
            if (String.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }

            var catalogKey = new CatalogKey(AppContext.Current.ApplicationId, code);

            return _priceService.GetPrices(marketId, DateTime.Now, catalogKey, priceFilter).OrderBy(x => x.UnitPrice.Amount).ToList();
        }

        public IList<IPriceValue> GetPriceList(IEnumerable<CatalogKey> catalogKeys, MarketId marketId, PriceFilter priceFilter)
        {
            if (catalogKeys == null)
            {
                throw new ArgumentNullException("catalogKeys");
            }

            return _priceService.GetPrices(marketId, DateTime.Now, catalogKeys, priceFilter).OrderBy(x => x.UnitPrice.Amount).ToList();
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

        public IList<IPriceValue> GetDiscountPriceList(IEnumerable<CatalogKey> catalogKeys, MarketId marketId)
        {
            return GetDiscountPriceList(catalogKeys, marketId, Currency.Empty);
        }

        public IList<IPriceValue> GetDiscountPriceList(IEnumerable<CatalogKey> catalogKeys, MarketId marketId, Currency currency)
        {
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
            var prices = catalogKeys.SelectMany(x => GetPriceList(x.CatalogEntryCode, marketId, priceFilter ));
            return GetDiscountPrices(prices.ToList(), marketId, currency);
        }
        public IPriceValue GetDiscountPrice(CatalogKey catalogKey, MarketId marketId, Currency currency)
        {
            return GetDiscountPriceList(new[] { catalogKey }, marketId, currency).FirstOrDefault();
        }

        private IList<IPriceValue> GetDiscountPrices(IList<IPriceValue> prices, MarketId marketId, Currency currency)
        {
            var priceValues = new List<IPriceValue>();
            var market = _marketService.GetMarket(marketId);
            var filter = new PromotionFilter()
            {
                IgnoreConditions = false,
                IgnorePolicy = false,
                IgnoreSegments = false,
                IncludeCoupons = false
            };

            var helper = new PromotionHelper
            {
                PromotionContext =
                {
                    TargetGroup = PromotionGroup.GetPromotionGroup(PromotionGroup.PromotionGroupKey.Entry).Key
                }
            };
           

            var entries = prices.GroupBy(x => x.CatalogKey.CatalogEntryCode)
                .Select(x => x.First())
                .Select(x => _catalogSystem.GetCatalogEntry(x.CatalogKey.CatalogEntryCode, new CatalogEntryResponseGroup(CatalogEntryResponseGroup.ResponseGroup.Nodes)));

            var currencies = new [] { currency };
            if (currency == Currency.Empty)
            {
                currencies = market.Currencies.Select(x => new Currency(x)).ToArray();
            }
            foreach (var entry in entries)
            {
                foreach (var currentCurrency in currencies)
                {
                    var catalogNodes = String.Empty;
                    var catalogs = String.Empty;

                    if (entry.Nodes != null && entry.Nodes.CatalogNode != null && entry.Nodes.CatalogNode.Length > 0)
                    {
                        foreach (var node in entry.Nodes.CatalogNode)
                        {
                            var entryCatalogName = CatalogContext.Current.GetCatalogDto(node.CatalogId).Catalog[0].Name;

                            if (String.IsNullOrEmpty(catalogs))
                            {
                                catalogs = entryCatalogName;
                            }
                            else
                            {
                                catalogs += ";" + entryCatalogName;
                            }
                            if (String.IsNullOrEmpty(catalogNodes))
                            {
                                catalogNodes = node.ID;
                            }
                            else
                            {
                                catalogNodes += ";" + node.ID;
                            }
                        }
                    }
                    
                    var price = prices.OrderBy(x => x.UnitPrice.Amount).FirstOrDefault(x => x.CatalogKey.CatalogEntryCode.Equals(entry.ID) && x.UnitPrice.Currency.Equals(currentCurrency));
                    if (price == null)
                    {
                        continue;
                    }
                    var result = new PromotionEntry(catalogs, catalogNodes, entry.ID, price.UnitPrice.Amount);
                    _promotionEntryPopulate.Populate(result, entry, marketId, currentCurrency);
                    var sourceSet = new PromotionEntriesSet();
                    sourceSet.Entries.Add(result);
                    helper.PromotionContext.SourceEntriesSet = sourceSet;
                    helper.PromotionContext.TargetEntriesSet = sourceSet;
                    helper.Eval(filter, false);
                    if (helper.PromotionContext.PromotionResult.PromotionRecords.Count > 0)
                    {
                        priceValues.Add(new PriceValue
                        {
                            CatalogKey = price.CatalogKey,
                            CustomerPricing = CustomerPricing.AllCustomers,
                            MarketId = price.MarketId,
                            MinQuantity = 1,
                            UnitPrice = new Money(price.UnitPrice.Amount - GetDiscountPrice(helper.PromotionContext.PromotionResult), currentCurrency),
                            ValidFrom = DateTime.UtcNow,
                            ValidUntil = null
                        });
                    }
                    else
                    {
                        priceValues.Add(price);
                    }
                }
            }
            return priceValues;
        }

        private decimal GetDiscountPrice(PromotionResult result)
        {
            return result.PromotionRecords.Sum(record => GetDiscountAmount(record, record.PromotionReward));
        }

        
        private decimal GetDiscountAmount(PromotionItemRecord record, PromotionReward reward)
        {
            decimal discountAmount = 0;
            if (reward.RewardType != PromotionRewardType.EachAffectedEntry && reward.RewardType != PromotionRewardType.AllAffectedEntries)
            {
                return Math.Round(discountAmount, 2);
            }
            if (reward.AmountType == PromotionRewardAmountType.Percentage)
            {
                discountAmount = record.AffectedEntriesSet.TotalCost * reward.AmountOff / 100;
            }
            else 
            {
                discountAmount += reward.AmountOff; 
            }
            return Math.Round(discountAmount, 2);
        }
    }
}