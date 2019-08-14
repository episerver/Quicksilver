using EPiServer.Reference.Commerce.Site.Features.TestImage.Blocks;
using EPiServer.Reference.Commerce.Site.Infrastructure.React;
using EPiServer.Web.Mvc;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.TestImage.Controllers
{
    public class TestImageBlockController : BlockController<TestImageBlock>
    {
        public override ActionResult Index(TestImageBlock currentBlock)
        {
            return Component.RenderJson(currentBlock);
        }
    }
}