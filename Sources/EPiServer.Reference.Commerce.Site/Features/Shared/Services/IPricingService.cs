using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Pricing;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Services
{
    public interface IPricingService
    {
        IList<IPriceValue> GetPriceList(string code, MarketId marketId, PriceFilter priceFilter);
        IList<IPriceValue> GetPriceList(IEnumerable<CatalogKey> catalogKeys, MarketId marketId, PriceFilter priceFilter);
        Money? GetCurrentPrice(string code);
        Money? GetPrice(string code, MarketId marketId, Currency currency);
    }
}
