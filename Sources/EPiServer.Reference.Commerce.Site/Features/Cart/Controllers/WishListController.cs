using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.Cart.Models;
using EPiServer.Reference.Commerce.Site.Features.Cart.Pages;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Product.Services;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.Controllers
{
    [Authorize]
    public class WishListController : PageController<WishListPage>
    {
        private readonly IContentLoader _contentLoader;
        private readonly ICartService _cartService;
        private readonly LocalizationService _localizationService;
        private readonly IProductService _productService;
        
        public WishListController(
            IContentLoader contentLoader,
            ICartService cartService,
            LocalizationService localizationService,
            IProductService productService)
        {
            _contentLoader = contentLoader;
            _localizationService = localizationService;
            _cartService = cartService;
            _productService = productService;
            _cartService.InitializeAsWishList();
        }

        [HttpGet]
        public ActionResult Index(WishListPage currentPage)
        {
            WishListViewModel viewModel = new WishListViewModel
            {
                ItemCount = _cartService.GetLineItemsTotalQuantity(),
                CurrentPage = currentPage,
                CartItems = _cartService.GetCartItems(),
                Total = _cartService.GetSubTotal()
            };

            return View(viewModel);
        }

        [AcceptVerbs(HttpVerbs.Get|HttpVerbs.Post)]
        public ActionResult WishListMiniCartDetails()
        {
            WishListMiniCartViewModel viewModel = new WishListMiniCartViewModel
            {
                ItemCount = _cartService.GetLineItemsTotalQuantity(),
                WishListPage = _contentLoader.Get<StartPage>(ContentReference.StartPage).WishListPage,
                CartItems = _cartService.GetCartItems(),
                Total = _cartService.GetTotal()
            };

            return PartialView("_WishListMiniCartDetails", viewModel);
        }

        [HttpPost]
        public ActionResult AddToCart(string code)
        {
            ModelState.Clear();
            string warningMessage = null;

            if (_cartService.AddToCart(code, out warningMessage))
            {
                return WishListMiniCartDetails();
            }

            // HttpStatusMessage can't be longer than 512 characters.
            warningMessage = warningMessage.Length < 512 ? warningMessage : warningMessage.Substring(512);
            return new HttpStatusCodeResult(500, warningMessage);
        }

        [HttpPost]
        public ActionResult ChangeCartItem(string code, decimal quantity, string size, string newSize)
        {
            ModelState.Clear();

            if (quantity > 0)
            {
                if (size == newSize)
                {
                    _cartService.ChangeQuantity(code, quantity);
                }
                else
                {
                    var newCode = _productService.GetSiblingVariantCodeBySize(code, newSize);
                    _cartService.UpdateLineItemSku(code, newCode, quantity);
                }
            }
            else
            {
                _cartService.RemoveLineItem(code);
            }

            return WishListMiniCartDetails();
        }

        [HttpPost]
        public ActionResult DeleteWishList()
        {
            _cartService.DeleteCart();
            var startPage = _contentLoader.Get<StartPage>(ContentReference.StartPage);

            return RedirectToAction("Index", new {Node = startPage.WishListPage});
        }
    }
}