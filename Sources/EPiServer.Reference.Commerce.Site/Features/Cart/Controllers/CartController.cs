using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Cart.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using System.Linq;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.Controllers
{
    public class CartController : Controller
    {
        private readonly IContentLoader _contentLoader;
        private readonly ICartService _cartService;
        private readonly IWishListService _wishListService;

        public CartController(IContentLoader contentLoader, ICartService cartService, IWishListService wishListService)
        {
            _contentLoader = contentLoader;
            _cartService = cartService;
            _wishListService = wishListService;
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult MiniCartSummary()
        {
            var model = new MiniCartSummaryViewModel
            {
                ItemCount = (int) _cartService.GetAllLineItems().Sum(x => x.Quantity)
            };
            return PartialView(model);
        }

        [HttpGet]
        public ActionResult MiniCartDetails()
        {
            var model = new MiniCartDetailsViewModel
                {
                    ItemCount = (int)_cartService.GetAllLineItems().Sum(x => x.Quantity),
                    CheckoutPage = _contentLoader.Get<StartPage>(ContentReference.StartPage).CheckoutPage,
                    CartItems = _cartService.GetCartItems(),
                    Total = _cartService.GetTotal()
                };
            return PartialView(model);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult LargeCart()
        {
            var model = new LargeCartViewModel
                {
                    CartItems = _cartService.GetCartItems(),
                    Total = _cartService.GetTotal(),
                    TotalDiscount = _cartService.GetTotalDiscount()
                };
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult AddToCart(string code)
        {
            _cartService.AddToCart(code);
            _wishListService.RemoveItem(code);
            return RedirectToAction("MiniCartDetails");
        }

        [HttpPost]
        public ActionResult ChangeQuantity(string code, int? quantity, bool miniCart = false)
        {
            if(quantity != null && quantity.Value > 0)
            {
                _cartService.ChangeQuantity(code, quantity.Value);
            }
            return miniCart ? RedirectToAction("MiniCartDetails") : RedirectToAction("LargeCart");
        }

        [HttpPost]
        public ActionResult RemoveLineItem(string code)
        {
            _cartService.RemoveLineItem(code);
            return RedirectToAction("LargeCart");
        }
    }
}