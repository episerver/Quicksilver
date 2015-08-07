using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Cart.Models;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Product.Services;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.Controllers
{
    public class CartController : Controller
    {
        private readonly IContentLoader _contentLoader;
        private readonly ICartService _cartService;
        private readonly ICartService _wishListService;
        private readonly IProductService _productService;

        public CartController(IContentLoader contentLoader,
                              ICartService cartService,
                              ICartService wishListService,
                              IProductService productService)
        {
            _contentLoader = contentLoader;
            _cartService = cartService;
            _wishListService = wishListService;
            _productService = productService;
            _wishListService.InitializeAsWishList();
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult MiniCartDetails()
        {
            var viewModel = new MiniCartViewModel
            {
                ItemCount = _cartService.GetLineItemsTotalQuantity(),
                CheckoutPage = _contentLoader.Get<StartPage>(ContentReference.StartPage).CheckoutPage,
                CartItems = _cartService.GetCartItems(),
                Total = _cartService.GetSubTotal()
            };
            
            return PartialView("_MiniCartDetails", viewModel);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult LargeCart()
        {
            var items = _cartService.GetCartItems().ToList();
            var viewModel = new LargeCartViewModel
            {
                CartItems = items,
                Total = _cartService.ConvertToMoney(items.Sum(x => x.ExtendedPrice.Amount)),
                TotalDiscount = _cartService.GetTotalDiscount()
            };

            return PartialView("LargeCart", viewModel);
        }

        [HttpPost]
        public ActionResult AddToCart(string code)
        {
            ModelState.Clear();
            string warningMessage = null;
        
            if (_cartService.AddToCart(code, out warningMessage))
            {
                _wishListService.RemoveLineItem(code);
                return MiniCartDetails();
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

            return MiniCartDetails();
        }
    }
}