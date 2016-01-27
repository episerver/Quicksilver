using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Reference.Commerce.Site.Tests.Features.Shared.Services;
using Mediachase.Commerce.Marketing;

namespace EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes
{
    public class FakePromotionHelper : PromotionHelperFacade
    {
        public override PromotionContext Evaluate(PromotionFilter filter, PromotionEntriesSet sourceEntriesSet, PromotionEntriesSet targetEntriesSet, bool checkEntryLevelLimit)
        {
            return null;
        }
    }
}