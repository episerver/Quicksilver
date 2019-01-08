using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Personalization.Commerce.Tracking;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Tracking.Commerce.Data;
using EPiServer.Web.Routing;
using Mediachase.Commerce.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Recommendations.Services
{
    public class TrackingResponseDataInterceptor : ITrackingResponseDataInterceptor
    {
        private readonly string Title = "Title";
        private readonly string Url = "Url";
        private readonly string Image = "Img";
        private readonly string UnitPrice = "UnitPrice";
        private readonly string SalePrice = "SalePrice";

        private readonly IContentLoader _contentLoader;
        private readonly IPricingService _pricingService;
        private readonly UrlResolver _urlResolver;
        private readonly CatalogContentService _catalogContentService;
        private readonly ReferenceConverter _referenceConverter;

        public TrackingResponseDataInterceptor(
            IContentLoader contentLoader,
            IPricingService pricingService,
            UrlResolver urlResolver,
            CatalogContentService catalogContentService,
            ReferenceConverter referenceConverter)
        {
            _contentLoader = contentLoader;
            _pricingService = pricingService;
            _urlResolver = urlResolver;
            _catalogContentService = catalogContentService;
            _referenceConverter = referenceConverter;
        }

        public void Intercept(TrackingResponseData data)
        {
            if (data == null || data.SmartRecs == null)
            {
                return;
            }

            var variantCodes = data.SmartRecs.SelectMany(s => s.Recs.Select(r => r.RefCode)).Distinct();
            var entries = _catalogContentService.GetItems<EntryContentBase>(variantCodes);

            foreach (var smartRect in data.SmartRecs)
            {
                foreach (var recommendation in smartRect.Recs)
                {
                    var entry = entries.SingleOrDefault(v => string.Equals(v.Code, recommendation.RefCode, StringComparison.OrdinalIgnoreCase));
                    var productContent = entry as ProductContent;
                    if (productContent != null)
                    {
                        entry = _catalogContentService.GetFirstVariant<VariationContent>(productContent);
                    }
                    if (entry == null)
                    {
                        continue;
                    }
                    UpdateRecommendation(recommendation, entry);
                }
            }
        }

        private void UpdateRecommendation(RecommendationData recommendation, EntryContentBase entry)
        {
            var imageUrl = entry.GetAssets<IContentImage>(_contentLoader, _urlResolver).FirstOrDefault() ?? "";
            var originalPrice = _pricingService.GetPrice(entry.Code);
            var salePrice = _pricingService.GetDiscountPrice(entry.Code);

            if (recommendation.Attributes == null)
            {
                recommendation.Attributes = new Dictionary<string, string>();
            }

            recommendation.Attributes.Add(Title, entry.DisplayName);
            recommendation.Attributes.Add(Url, entry.GetUrl(recommendation.Lang));
            recommendation.Attributes.Add(Image, imageUrl);
            recommendation.Attributes.Add(UnitPrice, originalPrice?.UnitPrice.ToString());
            recommendation.Attributes.Add(SalePrice, salePrice?.UnitPrice.ToString());
        }
    }
}