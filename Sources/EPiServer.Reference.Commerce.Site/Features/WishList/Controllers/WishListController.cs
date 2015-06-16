using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Features.WishList.Pages;
using EPiServer.Web.Mvc;
using System;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.WishList.Controllers
{
    [Authorize]
    public class WishListController : PageController<WishListPage>
    {
        private readonly IContentLoader _contentLoader;
        private readonly IWishListService _wishListService;
        private readonly LocalizationService _localizationService;
        
        public WishListController(
            IContentLoader contentLoader,
            IWishListService wishListService, 
            LocalizationService localizationService)
        {
            _contentLoader = contentLoader;
            _wishListService = wishListService;
            _localizationService = localizationService;
        }

        [HttpGet]
        public ActionResult Index(WishListPage currentPage)
        {
            return View(_wishListService.GetViewModel(currentPage));
        }

        [HttpPost]
        public ActionResult AddToWishList(string code)
        {
            string warningMessage;
            var added = _wishListService.AddItem(code, out warningMessage);
            var message = added
                ? _localizationService.GetString("/ProductPage/AddedToWishList")
                : String.Concat(_localizationService.GetString("/ProductPage/NotAddedToWishList"), ". ", warningMessage);

            return new JsonResult() { Data = new { added, message } };
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