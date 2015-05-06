using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Features.WishList.Pages;
using EPiServer.Web.Mvc;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.WishList.Controllers
{
    [Authorize]
    public class WishListController : PageController<WishListPage>
    {
        private readonly IContentLoader _contentLoader;
        private readonly IWishListService _wishListService;
        
        public WishListController(IContentLoader contentLoader,
                                 IWishListService wishListService)
        {
            _contentLoader = contentLoader;
            _wishListService = wishListService;
        }

        [HttpGet]
        public ActionResult Index(WishListPage currentPage)
        {
            return View(_wishListService.GetViewModel(currentPage));
        }

        [HttpPost]
        public ActionResult AddToWishList(string code)
        {
           _wishListService.AddItem(code);
            return null;
        }

        [HttpPost]
        public ActionResult RemoveFromWishList(string code)
        {
            _wishListService.RemoveItem(code);
            return null;
        }

        [HttpPost]
        public ActionResult DeleteWishList()
        {
            _wishListService.Delete();
            var startPage = _contentLoader.Get<StartPage>(ContentReference.StartPage);
            return RedirectToAction("Index", new {Node = startPage.WishListPage});
        }
    }
}