using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.Indexing
{
    public class IndexingHelper
    {
        public static string GetPriceField(MarketId marketId, Currency currency)
        {
            return $"listing_price_{marketId}_{currency}".ToLower();
        }

        public static string GetOriginalPriceField(MarketId marketId, Currency currency)
        {
            return $"original_price_{marketId}_{currency}".ToLower();
        }
    }
}