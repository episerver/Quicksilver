using System;
using System.Linq;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.Marketing;
using Mediachase.Commerce.Marketing.Objects;
using Mediachase.Commerce.Pricing;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Services
{
    [ServiceConfiguration(typeof(IPromotionEntryService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class PromotionEntryService : IPromotionEntryService
    {
        private readonly ILinksRepository _linksRepository;
        private readonly ICatalogSystem _catalogSystem;
        private readonly IContentLoader _contentLoader;
        private readonly IWarehouseInventoryService _inventoryService;
        private readonly IWarehouseRepository _warehouseRepository;

        public PromotionEntryService(
            ILinksRepository linksRepository,
            ICatalogSystem catalogSystem,
            IContentLoader contentLoader,
            IWarehouseInventoryService inventoryService,
            IWarehouseRepository warehouseRepository)
        {
            _contentLoader = contentLoader;
            _linksRepository = linksRepository;
            _catalogSystem = catalogSystem;
            _inventoryService = inventoryService;
            _warehouseRepository = warehouseRepository;
        }

        public IPriceValue GetDiscountPrice(IPriceValue price, EntryContentBase entry, Currency currency, PromotionHelperFacade promotionHelper)
        {
            var promotionEntry = CreatePromotionEntry(entry, price);
            var filter = new PromotionFilter
            {
                IgnoreConditions = false,
                IgnorePolicy = false,
                IgnoreSegments = false,
                IncludeCoupons = false
            };

            var sourceSet = new PromotionEntriesSet();
            sourceSet.Entries.Add(promotionEntry);
            var promotionContext = promotionHelper.Evaluate(filter, sourceSet, sourceSet, false);

            if (promotionContext.PromotionResult.PromotionRecords.Count > 0)
            {
                return new PriceValue
                {
                    CatalogKey = price.CatalogKey,
                    CustomerPricing = CustomerPricing.AllCustomers,
                    MarketId = price.MarketId,
                    MinQuantity = 1,
                    UnitPrice = new Money(price.UnitPrice.Amount - GetDiscountPrice(promotionContext), currency),
                    ValidFrom = DateTime.UtcNow,
                    ValidUntil = null
                };
            }
            return price;
        }

        private PromotionEntry CreatePromotionEntry(EntryContentBase entry, IPriceValue price)
        {
            var catalogNodes = string.Empty;
            var catalogs = string.Empty;
            foreach (var node in entry.GetNodeRelations(_linksRepository).Select(x => _contentLoader.Get<NodeContent>(x.Target)))
            {
                var entryCatalogName = _catalogSystem.GetCatalogDto(node.CatalogId).Catalog[0].Name;
                catalogs = string.IsNullOrEmpty(catalogs) ? entryCatalogName : catalogs + ";" + entryCatalogName;
                catalogNodes = string.IsNullOrEmpty(catalogNodes) ? node.Code : catalogNodes + ";" + node.Code;
            }
            var promotionEntry = new PromotionEntry(catalogs, catalogNodes, entry.Code, price.UnitPrice.Amount);
            Populate(promotionEntry, entry, price);
            return promotionEntry;
        }

        private decimal GetDiscountPrice(PromotionContext promotionContext)
        {
            var result = promotionContext.PromotionResult;
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

        private void Populate(PromotionEntry entry, EntryContentBase catalogEntry, IPriceValue price)
        {
            entry.Quantity = 1;
            entry.Owner = catalogEntry;
            entry["Id"] = catalogEntry.Code;

            if (catalogEntry.Property != null)
            {
                foreach (var prop in catalogEntry.Property.Where(x => x.IsPropertyData))
                {
                    entry[prop.Name] = prop.Value;
                }
            }

            entry["ExtendedPrice"] = price.UnitPrice.Amount;
            var inventories = _inventoryService.List(price.CatalogKey, _warehouseRepository.List()).ToList();
            if (!inventories.Any())
            {
                return;
            }

            entry["AllowBackordersAndPreorders"] = inventories.Any(i => i.AllowBackorder) && inventories.Any(i => i.AllowPreorder);
            entry["InStockQuantity"] = inventories.Sum(i => i.InStockQuantity - i.ReservedQuantity);
            entry["PreorderQuantity"] = inventories.Sum(i => i.PreorderQuantity);
            entry["BackorderQuantity"] = inventories.Sum(i => i.BackorderQuantity);
            entry["InventoryStatus"] = inventories.First().InventoryStatus;
        }
    }
}