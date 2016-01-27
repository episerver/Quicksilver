using EPiServer.ServiceLocation;
using Mediachase.Commerce.Marketing;
using Mediachase.Commerce.Website.Helpers;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.Facades
{
    [ServiceConfiguration(typeof(PromotionHelperFacade), Lifecycle = ServiceInstanceScope.Singleton)]
    public class PromotionHelperFacade
    {
        public virtual PromotionContext Evaluate(PromotionFilter filter, PromotionEntriesSet sourceEntriesSet, PromotionEntriesSet targetEntriesSet, bool checkEntryLevelLimit)
        {
            var helper = new PromotionHelper();
            helper.PromotionContext.TargetGroup = PromotionGroup.GetPromotionGroup(PromotionGroup.PromotionGroupKey.Entry).Key;
            helper.PromotionContext.SourceEntriesSet = sourceEntriesSet;
            helper.PromotionContext.TargetEntriesSet = targetEntriesSet;
            helper.Eval(filter, checkEntryLevelLimit);
            return helper.PromotionContext;
        }
    }
}