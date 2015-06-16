using EPiServer.Reference.Commerce.Site.Features.Editorial.Blocks;
using EPiServer.Web.Mvc;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Editorial.Controllers
{
    public class FreeTextBlockController : BlockController<FreeTextBlock>
    {
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public override ActionResult Index(FreeTextBlock currentBlock)
        {
            return PartialView(currentBlock);
        }
    }
}