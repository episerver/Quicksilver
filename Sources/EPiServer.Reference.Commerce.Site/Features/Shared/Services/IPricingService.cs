using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Pricing;
using System.Collections.Generic;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Services
{
    public interface IPricingService
    {
        IPriceValue GetPrice(string code);
        IPriceValue GetPrice(string code, MarketId marketId, Currency currency);
        IPriceValue GetDefaultPrice(string code);
        IEnumerable<IPriceValue> GetCatalogEntryPrices(IEnumerable<CatalogKey> catalogKeys);
        IPriceValue GetDiscountPrice(string code);
        IPriceValue GetDiscountPrice(CatalogKey catalogKey, MarketId marketId, Currency currency);
        Money GetMoney(decimal amount);
        Money GetDiscountedPrice(ILineItem lineItem, Currency currency);
    }
}
