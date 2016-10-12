using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.Indexing
{
    public class IndexingHelper
    {
        public static string GetPriceField(MarketId marketId, Currency currency)
        {
            return string.Format("listing_price_{0}_{1}", marketId, currency).ToLower();
        }

        public static string GetOriginalPriceField(MarketId marketId, Currency currency)
        {
            return string.Format("original_price_{0}_{1}", marketId, currency).ToLower();
        }
    }
}