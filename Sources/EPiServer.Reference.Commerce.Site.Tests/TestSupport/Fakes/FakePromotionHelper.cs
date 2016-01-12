using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Reference.Commerce.Site.Tests.Features.Shared.Services;
using Mediachase.Commerce.Marketing;

namespace EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes
{
    public class FakePromotionHelper : PromotionHelperFacade
    {
        private readonly PromotionContext _promotionContext;

        public FakePromotionHelper()
        {
            _promotionContext = new PromotionContext(null, null, null);
        }

        public override PromotionContext PromotionContext
        {
            get { return _promotionContext; }
        }

        public override void Evaluate(PromotionFilter filter, bool checkEntryLevelLimit)
        {
            
        }
    }
}