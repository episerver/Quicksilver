using EPiServer.Reference.Commerce.Site.Features.Folder.Editorial.Blocks;
using EPiServer.Reference.Commerce.Site.Infrastructure.React;
using EPiServer.Web.Mvc;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Folder.Editorial.Controllers
{
    public class FreeTextBlockController : BlockController<FreeTextBlock>
    {
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public override ActionResult Index(FreeTextBlock currentBlock)
        {
            return Component.RenderJson(currentBlock);
        }
    }
}