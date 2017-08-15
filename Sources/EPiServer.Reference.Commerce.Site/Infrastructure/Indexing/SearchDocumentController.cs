using EPiServer.Commerce.Catalog;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Shared.CatalogIndexer;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using Mediachase.Commerce.Catalog;
using Mediachase.Search.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.Indexing
{
    [RoutePrefix("referenceapi")]
    public class SearchDocumentController : ApiController
    {
        private readonly IPricingService _pricingService;
        private readonly CatalogContentService _catalogContentService;
        private readonly AssetUrlResolver _assetUrlResolver;

        public SearchDocumentController(IPricingService pricingService,
            CatalogContentService catalogContentService,
            AssetUrlResolver assetUrlResolver)
        {
            _pricingService = pricingService;
            _catalogContentService = catalogContentService;
            _assetUrlResolver = assetUrlResolver;
        }

        [Route("searchdocuments/{language}/{code}", Name = "PopulateSearchDocument")]
        [AcceptVerbs("GET")]
        [ResponseType(typeof(RestSearchDocument))]
        public IHttpActionResult PopulateSearchDocument(string language, string code)
        {
            var model = PopulateRestSearchDocument(code, language);
            if (model == null)
            {
                return NotFound();
            }
            return Ok(model);
        }

        protected RestSearchDocument PopulateRestSearchDocument(string code, string language)
        {
            EntryContentBase entryContent;
            if (!_catalogContentService.TryGet(code, out entryContent))
            {
                return null;
            }
           
            var document = new RestSearchDocument();
            var fashionProduct = entryContent as FashionProduct;
            var fashionPackage = entryContent as FashionPackage;
            if (fashionProduct != null)
            {
                var variants = _catalogContentService.GetAllVariants<FashionVariant>(fashionProduct, language).ToList();
                AddPrices(document, variants);
                AddColors(document, variants);
                AddSizes(document, variants);
                AddCodes(document, variants);
                document.Fields.Add(new RestSearchField("brand", fashionProduct.Brand));
            }
            else if (fashionPackage != null)
            {
                AddPrices(document, new [] { fashionPackage });
            }
            document.Fields.Add(new RestSearchField("code", entryContent.Code, new[] { SearchField.Store.YES, SearchField.IncludeInDefaultSearch.YES }));
            document.Fields.Add(new RestSearchField("displayname", entryContent.DisplayName));
            document.Fields.Add(new RestSearchField("image_url", _assetUrlResolver.GetAssetUrl<IContentImage>(entryContent)));
            document.Fields.Add(new RestSearchField("content_link", entryContent.ContentLink.ToString()));
            document.Fields.Add(new RestSearchField("created", entryContent.Created.ToString("yyyyMMddhhmmss")));
            document.Fields.Add(new RestSearchField("top_category_name", _catalogContentService.GetTopCategoryName(entryContent)));

            return document;
        }
        
        private void AddSizes(RestSearchDocument document, IEnumerable<FashionVariant> variants)
        {
            var sizes = new List<string>();
            foreach (var fashionVariant in variants)
            {
                if (!String.IsNullOrEmpty(fashionVariant.Size) && !sizes.Contains(fashionVariant.Size.ToLower()))
                {
                    sizes.Add(fashionVariant.Size.ToLower());
                    document.Fields.Add(new RestSearchField("size", fashionVariant.Size.ToLower()));
                }
            }
        }

        private void AddColors(RestSearchDocument document, IEnumerable<FashionVariant> variants)
        {
            var colors = new List<string>();
            foreach (var fashionVariant in variants)
            {
                if (!String.IsNullOrEmpty(fashionVariant.Color) && !colors.Contains(fashionVariant.Color.ToLower()))
                {
                    colors.Add(fashionVariant.Color.ToLower());
                    document.Fields.Add(new RestSearchField("color", fashionVariant.Color.ToLower()));
                }
            }
        }

        private void AddPrices(RestSearchDocument document, IEnumerable<EntryContentBase> entries)
        {
            var prices = _pricingService.GetCatalogEntryPrices(entries.Select(x => new CatalogKey(x.Code))).ToList();
            var validPrices = prices.Where(x => x.ValidFrom <= DateTime.Now && (x.ValidUntil == null || x.ValidUntil >= DateTime.Now));

            foreach (var marketPrices in validPrices.GroupBy(x => x.MarketId))
            {
                foreach (var currencyPrices in marketPrices.GroupBy(x => x.UnitPrice.Currency))
                {
                    var topPrice = currencyPrices.OrderByDescending(x => x.UnitPrice).FirstOrDefault();
                    if (topPrice == null)
                        continue;

                    var variationPrice = new RestSearchField(IndexingHelper.GetOriginalPriceField(topPrice.MarketId, topPrice.UnitPrice.Currency),
                        topPrice.UnitPrice.Amount.ToString(CultureInfo.InvariantCulture), true);

                    var discountedPrice = new RestSearchField(IndexingHelper.GetPriceField(topPrice.MarketId, topPrice.UnitPrice.Currency),
                        _pricingService.GetDiscountPrice(topPrice.CatalogKey, topPrice.MarketId, topPrice.UnitPrice.Currency).UnitPrice.Amount.ToString(CultureInfo.InvariantCulture), true);

                    document.Fields.Add(variationPrice);
                    document.Fields.Add(discountedPrice);
                }
            }
        }

        private void AddCodes(RestSearchDocument document, IEnumerable<FashionVariant> variants)
        {
            foreach (var variant in variants)
            {
                document.Fields.Add(new RestSearchField("code", variant.Code, new[] { SearchField.Store.YES, SearchField.IncludeInDefaultSearch.YES }));
            }
        }
    }
}