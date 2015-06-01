using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Globalization;
using EPiServer.Logging;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Dto;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Pricing;
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
        private Injected<IPriceService> _priceService;
        private Injected<IPricingService> _pricingService;
        private Injected<IContentLoader> _contentLoader;
        private Injected<ReferenceConverter> _referenceConverter;

        private readonly ILogger _log = LogManager.GetLogger(typeof(CatalogIndexer));

        /// <summary>
        ///     Called when a catalog entry is indexed.
        ///     We use this method to load the prices for the variants of a product and store
        ///     the highest variant price on the product for optimal retrieval in the product listing.
        /// </summary>
        protected override void OnCatalogEntryIndex(ref SearchDocument document, CatalogEntryDto.CatalogEntryRow entry, string language)
        {
            if (entry.ClassTypeId == "Product")
            {
                var sw = new Stopwatch();
                sw.Start();
                var contentLink = _referenceConverter.Service.GetContentLink(entry.Code);
                var productContent = _contentLoader.Service.Get<FashionProduct>(contentLink);
                var variants = _contentLoader.Service.GetItems(productContent.GetVariants(), ContentLanguage.PreferredCulture).OfType<FashionVariant>().ToList();

                AddPrices(document, variants);
                AddColors(document, variants);
                AddSizes(document, variants);
                AddCodes(document, variants);
                document.Add(new SearchField("code", productContent.Code, new string[] { SearchField.Store.YES, SearchField.IncludeInDefaultSearch.YES }));
                document.Add(new SearchField("displayname", productContent.DisplayName));
                document.Add(new SearchField("image_url", productContent.GetDefaultAsset()));
                document.Add(new SearchField("content_link", productContent.ContentLink.ToString()));
                document.Add(new SearchField("created", productContent.Created.ToString("yyyyMMddhhmmss")));
                document.Add(new SearchField("brand", productContent.Brand));
                document.Add(new SearchField("top_category_name", GetTopCategory(productContent).DisplayName));

                sw.Stop();
                _log.Debug(string.Format("Indexing of {0} for {1} took {2}", productContent.Code, language, sw.Elapsed.Milliseconds));
            }
        }

        private NodeContent GetTopCategory(CatalogContentBase nodeContent)
        {
            var category = _contentLoader.Service.Get<CatalogContentBase>(nodeContent.ParentLink);
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
            var prices = _priceService.Service.GetCatalogEntryPrices(variants.Select(x => new CatalogKey(AppContext.Current.ApplicationId, x.Code))).ToList();
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
                        _pricingService.Service.GetDiscountPrice(topPrice.CatalogKey, topPrice.MarketId, topPrice.UnitPrice.Currency).UnitPrice.Amount);

                    document.Add(variationPrice);
                    document.Add(discountPrice);
                }
            }
        }

        private void AddCodes(ISearchDocument document, IEnumerable<FashionVariant> variants)
        {
            foreach (var variant in variants)
            {
                document.Add(new SearchField("code", variant.Code, new string[] { SearchField.Store.YES, SearchField.IncludeInDefaultSearch.YES }));
            }
        }
    }
}