using System;
using EPiServer.Business.Commerce;
using EPiServer.Commerce.Marketing;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Market.Services
{
    /// <summary>
    /// Gets the promotions and valid items for each promotion for the current market.
    /// </summary>
    /// <remarks>
    ///     Extract some common methods in <cref name="PromotionEngineContentLoader" /> class to this.
    /// </remarks>
    [ServiceConfiguration(typeof(MarketContentLoader), Lifecycle = ServiceInstanceScope.Singleton)]
    public class MarketContentLoader
    {
        private readonly IContentLoader _contentLoader;

        private readonly CampaignInfoExtractor _campaignInfoExtractor;
        private readonly PromotionProcessorResolver _promotionProcessorResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketContentLoader" /> class.
        /// </summary>
        /// <param name="contentLoader">Service used to load content data.</param>
        /// <param name="campaignInfoExtractor">Used to extract informations about campaigns and promotions.</param>
        /// <param name="promotionProcessorResolver">The promotion processor resolver.</param>
        public MarketContentLoader(
            IContentLoader contentLoader,
            CampaignInfoExtractor campaignInfoExtractor,
            PromotionProcessorResolver promotionProcessorResolver)
        {
            _contentLoader = contentLoader;

            _campaignInfoExtractor = campaignInfoExtractor;
            _promotionProcessorResolver = promotionProcessorResolver;
        }

        /// <summary>
        /// Gets the promotions and valid items for each promotion for <paramref name="market" />.
        /// </summary>
        /// <param name="market">The current market.</param>
        /// <returns>
        /// Collection of promotions and the valid items for each promotion
        /// </returns>
        public virtual IEnumerable<PromotionItems> GetPromotionItemsForMarket(IMarket market)
        {
            return GetEvaluablePromotionsInPriorityOrderForMarket(market)
                .Select(promotion => _promotionProcessorResolver.ResolveForPromotion(promotion).GetPromotionItems(promotion));
        }

        /// <summary>
        /// Gets the evaluable promotions in priority order.
        /// </summary>
        /// <param name="market">The current market.</param>
        /// <returns>All valid promotions for a given market, sorted by priority.</returns>
        public virtual IList<PromotionData> GetEvaluablePromotionsInPriorityOrderForMarket(IMarket market)
        {
            return GetPromotions().Where(x => IsValid(x, market)).OrderBy(x => x.Priority).ToList();
        }

        /// <summary>
        /// Gets all existing promotions.
        /// </summary>
        /// <returns>All <see cref="PromotionData" /> for all campaigns.</returns>
        public virtual IEnumerable<PromotionData> GetPromotions()
        {
            var campaigns = _contentLoader.GetChildren<SalesCampaign>(GetCampaignFolderRoot());
            var promotions = new List<PromotionData>();

            foreach (var campaign in campaigns)
            {
                promotions.AddRange(_contentLoader.GetChildren<PromotionData>(campaign.ContentLink));
            }

            return promotions;
        }

        /// <summary>
        /// Get the link to the campaign folder root.
        /// </summary>
        /// <returns>The content link to the campaign folder root.</returns>
        protected virtual ContentReference GetCampaignFolderRoot()
        {
            return SalesCampaignFolder.CampaignRoot;
        }

        private bool IsValid(PromotionData promotion, IMarket market)
        {
            var campaign = _contentLoader.Get<SalesCampaign>(promotion.ParentLink);

            return IsActive(promotion, campaign) && IsValidMarket(campaign, market);
        }

        private bool IsActive(PromotionData promotion, SalesCampaign campaign)
        {
            var status = _campaignInfoExtractor.GetEffectiveStatus(promotion, campaign);

            return status == CampaignItemStatus.Active;
        }

        private bool IsValidMarket(SalesCampaign campaign, IMarket market)
        {
            if (market == null)
            {
                return true;
            }

            if (!market.IsEnabled)
            {
                return false;
            }

            return campaign.TargetMarkets?.Contains(market.MarketId.Value, StringComparer.OrdinalIgnoreCase) ?? false;
        }
    }
}