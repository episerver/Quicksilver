using EPiServer.ServiceLocation;
using Mediachase.Commerce.Marketing;
using Mediachase.Commerce.Website.Helpers;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.Facades
{
    [ServiceConfiguration(typeof(PromotionHelperFacade), Lifecycle = ServiceInstanceScope.Singleton)]
    public class PromotionHelperFacade
    {
        private PromotionHelper _helper;

        public virtual PromotionContext PromotionContext
        {
            get { return ServiceLocator.Current.GetInstance<PromotionHelper>().PromotionContext; }
        }

        public virtual void Evaluate(PromotionFilter filter, bool checkEntryLevelLimit)
        {
            _helper = ServiceLocator.Current.GetInstance<PromotionHelper>();
            _helper.PromotionContext.TargetGroup = PromotionGroup.GetPromotionGroup(PromotionGroup.PromotionGroupKey.Entry).Key;
            _helper.Eval(filter, checkEntryLevelLimit);
        }
    }
}