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
            if (!entry.ClassTypeId.Equals(EntryType.Product))
            {
                return;
            }
            UpdateSearchDocument(ref document, entry, language);
        }

        public void UpdateSearchDocument(ref SearchDocument document, CatalogEntryDto.CatalogEntryRow entry, string language)
        {
            var sw = new Stopwatch();
            sw.Start();
            var contentLink = _referenceConverter.GetContentLink(entry.Code);
            var productContent = _contentLoader.Get<FashionProduct>(contentLink);
            var variants = _contentLoader.GetItems(productContent.GetVariants(_relationRepository), CultureInfo.GetCultureInfo(language)).OfType<FashionVariant>().ToList();

            AddPrices(document, variants);
            AddColors(document, variants);
            AddSizes(document, variants);
            AddCodes(document, variants);
            document.Add(new SearchField("code", productContent.Code, new[] { SearchField.Store.YES, SearchField.IncludeInDefaultSearch.YES }));
            document.Add(new SearchField("displayname", productContent.DisplayName));
            document.Add(new SearchField("image_url", _assetUrlResolver.GetAssetUrl<IContentImage>(productContent)));
            document.Add(new SearchField("content_link", productContent.ContentLink.ToString()));
            document.Add(new SearchField("created", productContent.Created.ToString("yyyyMMddhhmmss")));
            document.Add(new SearchField("brand", productContent.Brand));
            document.Add(new SearchField("top_category_name", GetTopCategory(productContent).DisplayName));

            sw.Stop();
            _log.Debug(string.Format("Indexing of {0} for {1} took {2}", productContent.Code, language, sw.Elapsed.Milliseconds));
        }

        private NodeContent GetTopCategory(CatalogContentBase nodeContent)
        {
            var category = _contentLoader.Get<CatalogContentBase>(nodeContent.ParentLink);
            if (category.ContentType == CatalogContentType.Catalog)
            { 
                return (NodeContent)nodeContent;
            }
            return GetTopCategory(category);
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

        private void AddPrices(ISearchDocument document, IEnumerable<FashionVariant> variants)
        {
            var prices = _priceService.GetCatalogEntryPrices(variants.Select(x => new CatalogKey(_appContext.ApplicationId, x.Code))).ToList();
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

                    var discountPrice = new SearchField(IndexingHelper.GetPriceField(topPrice.MarketId, topPrice.UnitPrice.Currency),
                        _promotionService.GetDiscountPrice(topPrice.CatalogKey, topPrice.MarketId, topPrice.UnitPrice.Currency).UnitPrice.Amount);

                    document.Add(variationPrice);
                    document.Add(discountPrice);
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
    }
}