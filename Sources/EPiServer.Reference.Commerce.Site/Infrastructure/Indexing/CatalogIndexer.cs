using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Core;
using EPiServer.Logging;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.ServiceLocation;
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
using System.Globalization;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.Indexing
{
    public class CatalogIndexer : CatalogIndexBuilder
    {
        private readonly IPriceService _priceService;
        private readonly IPromotionService _promotionService;
        private readonly IContentLoader _contentLoader;
        private readonly ReferenceConverter _referenceConverter;
        private readonly AssetUrlResolver _assetUrlResolver;
        private readonly IRelationRepository _relationRepository;
        private readonly AppContextFacade _appContext;
        private readonly ILogger _log;

        public CatalogIndexer()
        {
            _priceService = ServiceLocator.Current.GetInstance<IPriceService>();
            _contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            _promotionService = ServiceLocator.Current.GetInstance<IPromotionService>();
            _referenceConverter = ServiceLocator.Current.GetInstance<ReferenceConverter>();
            _assetUrlResolver = ServiceLocator.Current.GetInstance<AssetUrlResolver>();
            _relationRepository = ServiceLocator.Current.GetInstance<IRelationRepository>();
            _appContext = ServiceLocator.Current.GetInstance<AppContextFacade>();
            _log = LogManager.GetLogger(typeof(CatalogIndexer));
        }

        public CatalogIndexer(ICatalogSystem catalogSystem,
            IPriceService priceService,
            IInventoryService inventoryService,
            MetaDataContext metaDataContext,
            IContentLoader contentLoader,
            IPromotionService promotionService,
            ReferenceConverter referenceConverter,
            AssetUrlResolver assetUrlResolver,
            IRelationRepository relationRepository,
            AppContextFacade appContext,
            ILogger logger)
            : base(catalogSystem, priceService, inventoryService, metaDataContext)
        {
            _priceService = priceService;
            _contentLoader = contentLoader;
            _promotionService = promotionService;
            _referenceConverter = referenceConverter;
            _assetUrlResolver = assetUrlResolver;
            _relationRepository = relationRepository;
            _appContext = appContext;
            _log = logger;
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
            var languageCulture = CultureInfo.GetCultureInfo(language);
            var contentLink = _referenceConverter.GetContentLink(entryCode);
            var entryContent = _contentLoader.Get<EntryContentBase>(contentLink, languageCulture);
            var relatedProducts = GetProducts(entryContent, languageCulture).ToArray();
            var relatedVariants = GetVariants(relatedProducts, languageCulture).ToArray();

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
            document.Add(new SearchField("top_category_name", GetTopCategoryName(entryContent)));

            stopwatch.Stop();
            _log.Debug(string.Format("Indexing of {0} for {1} took {2}", entryContent.Code, language, stopwatch.Elapsed.Milliseconds));
        }

        private IEnumerable<FashionVariant> GetVariants(IEnumerable<FashionProduct> products, CultureInfo language)
        {
            var variants = products
                .SelectMany(x => _contentLoader.GetItems(x.GetVariants(_relationRepository), language)
                .OfType<FashionVariant>());

            return variants;
        }

        private IEnumerable<FashionProduct> GetProducts(EntryContentBase entryContent, CultureInfo language)
        {
            switch (entryContent.ClassTypeId)
            {
                case EntryType.Package:
                    return _contentLoader.GetItems(((PackageContent)entryContent).GetEntries(), language).OfType<FashionProduct>();

                case EntryType.Bundle:
                    return _contentLoader.GetItems(((BundleContent)entryContent).GetEntries(), language).OfType<FashionProduct>();

                case EntryType.Product:
                    return new[] { entryContent as FashionProduct };
            }

            return Enumerable.Empty<FashionProduct>();
        }

        private string GetTopCategoryName(EntryContentBase content)
        {
            var parent = _contentLoader.Get<CatalogContentBase>(content.ParentLink);
            var catalog = parent as CatalogContent;
            if (catalog != null)
            {
                return catalog.Name;
            }

            var node = parent as NodeContent;
            return node != null ? GetTopCategory(node).DisplayName : String.Empty;
        }

        private NodeContent GetTopCategory(NodeContent node)
        {
            var parentNode = _contentLoader.Get<CatalogContentBase>(node.ParentLink) as NodeContent;
            return parentNode != null ? GetTopCategory(parentNode) : node;
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
            var prices = _priceService.GetCatalogEntryPrices(skuEntries.Select(x => new CatalogKey(_appContext.ApplicationId, x.Code))).ToList();
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
                        _promotionService.GetDiscountPrice(topPrice.CatalogKey, topPrice.MarketId, topPrice.UnitPrice.Currency).UnitPrice.Amount);

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