using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using Mediachase.Commerce;
using Mediachase.Commerce.Marketing;
using Mediachase.Commerce.Marketing.Objects;
using Mediachase.Commerce.Pricing;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Services
{
    public interface IPromotionEntryService
    {
        IPriceValue GetDiscountPrice(IPriceValue price, EntryContentBase entry, Currency currency,
            PromotionHelperFacade promotionHelper);
        
    }
}
