using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Logging;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Dto;
using Mediachase.Commerce.Catalog.Objects;
using Mediachase.Commerce.InventoryService;
using Mediachase.Commerce.Pricing;
using Mediachase.MetaDataPlus;
using Mediachase.Search;
using Mediachase.Search.Extensions;
using Mediachase.Search.Extensions.Indexers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.Indexing
{
    public class CatalogIndexer : CatalogIndexBuilder
    {
        private readonly IPricingService _pricingService;
        private readonly CatalogContentService _catalogContentService;
        private readonly AssetUrlResolver _assetUrlResolver;
        private readonly ILogger _log;

        public CatalogIndexer()
            : this(
                ServiceLocator.Current.GetInstance<ICatalogSystem>(),
                ServiceLocator.Current.GetInstance<IPriceService>(),
                ServiceLocator.Current.GetInstance<IPricingService>(),
                ServiceLocator.Current.GetInstance<IInventoryService>(),
                ServiceLocator.Current.GetInstance<MetaDataContext>(),
                ServiceLocator.Current.GetInstance<CatalogItemChangeManager>(),
                ServiceLocator.Current.GetInstance<NodeIdentityResolver>(),
                ServiceLocator.Current.GetInstance<AssetUrlResolver>(),
                ServiceLocator.Current.GetInstance<CatalogContentService>())
        {
        }

        public CatalogIndexer(
            ICatalogSystem catalogSystem,
            IPriceService priceService,
            IPricingService pricingService,
            IInventoryService inventoryService,
            MetaDataContext metaDataContext,
            CatalogItemChangeManager catalogItemChangeManager,
            NodeIdentityResolver nodeIdentityResolver,
            AssetUrlResolver assetUrlResolver,
            CatalogContentService catalogContentService)
            : base(
                catalogSystem,
                priceService,
                inventoryService,
                metaDataContext,
                catalogItemChangeManager,
                nodeIdentityResolver)
        {
            _pricingService = pricingService;
            _assetUrlResolver = assetUrlResolver;
            _catalogContentService = catalogContentService;
            _log = LogManager.GetLogger(typeof(CatalogIndexer));
        }

        /// <summary>
        ///     Called when a catalog entry is indexed.
        ///     We use this method to load the prices for the variants of a product and store
        ///     the highest variant price on the product for optimal retrieval in the product listing.
        /// </summary>
        protected override void OnCatalogEntryIndex(ref SearchDocument document, CatalogEntryDto.CatalogEntryRow entry, string language)
        {
            switch (entry.ClassTypeId)
            {
                case EntryType.Package:
                case EntryType.Bundle:
                case EntryType.Product:
                    UpdateSearchDocument(ref document, entry.Code, language);
                    break;
            }
        }

        public void UpdateSearchDocument(ref SearchDocument document, string entryCode, string language)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var entryContent = _catalogContentService.Get<EntryContentBase>(entryCode, language);
            var relatedProducts = _catalogContentService.GetProducts(entryContent, language).ToArray();
            var relatedVariants = _catalogContentService.GetAllVariants<FashionVariant>(relatedProducts, language).ToArray();

            AddPrices(document, new[] { entryContent });
            AddCodes(document, relatedVariants);
            AddBrands(document, relatedProducts);
            AddPrices(document, relatedVariants);
            AddColors(document, relatedVariants);
            AddSizes(document, relatedVariants);

            document.Add(new SearchField("code", entryContent.Code, new[] { SearchField.Store.YES, SearchField.IncludeInDefaultSearch.YES }));
            document.Add(new SearchField("displayname", entryContent.DisplayName));
            document.Add(new SearchField("image_url", _assetUrlResolver.GetAssetUrl<IContentImage>(entryContent)));
            document.Add(new SearchField("content_link", entryContent.ContentLink.ToString()));
            document.Add(new SearchField("created", entryContent.Created.ToString("yyyyMMddhhmmss")));
            document.Add(new SearchField("top_category_name", _catalogContentService.GetTopCategoryName(entryContent)));

            stopwatch.Stop();
            _log.Debug($"Indexing of {entryContent.Code} for {language} took {stopwatch.Elapsed.Milliseconds}");
        }
        
        private void AddSizes(ISearchDocument document, IEnumerable<FashionVariant> variants)
        {
            var sizes = new List<string>();
            foreach (var fashionVariant in variants)
            {
                if (!String.IsNullOrEmpty(fashionVariant.Size) && !sizes.Contains(fashionVariant.Size.ToLower()))
                {
                    sizes.Add(fashionVariant.Size.ToLower());
                    document.Add(new SearchField("size", fashionVariant.Size.ToLower()));
                }
            }
        }

        private void AddColors(ISearchDocument document, IEnumerable<FashionVariant> variants)
        {
            var colors = new List<string>();
            foreach (var fashionVariant in variants)
            {
                if (!String.IsNullOrEmpty(fashionVariant.Color) && !colors.Contains(fashionVariant.Color.ToLower()))
                {
                    colors.Add(fashionVariant.Color.ToLower());
                    document.Add(new SearchField("color", fashionVariant.Color.ToLower()));
                }
            }
        }

        private void AddPrices(ISearchDocument document, IEnumerable<EntryContentBase> skuEntries)
        {
            var prices = _pricingService.GetCatalogEntryPrices(skuEntries.Select(x => new CatalogKey(x.Code))).ToList();
            var validPrices = prices.Where(x => x.ValidFrom <= DateTime.Now && (x.ValidUntil == null || x.ValidUntil >= DateTime.Now));

            foreach (var marketPrices in validPrices.GroupBy(x => x.MarketId))
            {
                foreach (var currencyPrices in marketPrices.GroupBy(x => x.UnitPrice.Currency))
                {
                    var topPrice = currencyPrices.OrderByDescending(x => x.UnitPrice).FirstOrDefault();
                    if (topPrice == null)
                        continue;

                    var variationPrice = new SearchField(IndexingHelper.GetOriginalPriceField(topPrice.MarketId, topPrice.UnitPrice.Currency),
                        topPrice.UnitPrice.Amount);

                    var discountedPrice = new SearchField(IndexingHelper.GetPriceField(topPrice.MarketId, topPrice.UnitPrice.Currency),
                        _pricingService.GetDiscountPrice(topPrice.CatalogKey, topPrice.MarketId, topPrice.UnitPrice.Currency).UnitPrice.Amount);

                    document.Add(variationPrice);
                    document.Add(discountedPrice);
                }
            }
        }

        private void AddCodes(ISearchDocument document, IEnumerable<FashionVariant> variants)
        {
            foreach (var variant in variants)
            {
                document.Add(new SearchField("code", variant.Code, new[] { SearchField.Store.YES, SearchField.IncludeInDefaultSearch.YES }));
            }
        }

        private void AddBrands(ISearchDocument document, IEnumerable<FashionProduct> products)
        {
            foreach (var brand in products
                .Where(x => !String.IsNullOrEmpty(x.Brand))
                .Select(x => x.Brand.ToLower())
                .Distinct())
            {
                document.Add(new SearchField("brand", brand));
            }
        }
    }
}