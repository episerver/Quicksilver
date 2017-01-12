using EPiServer.Reference.Commerce.Site.Features.Folder.Editorial.Blocks;
using EPiServer.Web.Mvc;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Folder.Editorial.Controllers
{
    public class MapBlockController : BlockController<MapBlock>
    {
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public override ActionResult Index(MapBlock currentBlock)
        {
            return PartialView(currentBlock);
        }
    }
}