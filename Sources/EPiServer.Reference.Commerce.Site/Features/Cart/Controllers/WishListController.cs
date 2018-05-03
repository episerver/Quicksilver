using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Cart.Pages;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Services;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using EPiServer.Tracking.Commerce;
using EPiServer.Web.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.Controllers
{
    [Authorize]
    public class WishListController : PageController<WishListPage>
    {
        private readonly IContentLoader _contentLoader;
        private readonly ICartService _cartService;
        private ICart _wishlist;
        private readonly IOrderRepository _orderRepository;
        private readonly IRecommendationService _recommendationService;
        readonly CartViewModelFactory _cartViewModelFactory;

        public WishListController(
            IContentLoader contentLoader,
            ICartService cartService,
            IOrderRepository orderRepository,
            IRecommendationService recommendationService,
            CartViewModelFactory cartViewModelFactory)
        {
            _contentLoader = contentLoader;
            _cartService = cartService;
            _orderRepository = orderRepository;
            _recommendationService = recommendationService;
            _cartViewModelFactory = cartViewModelFactory;
        }

        [HttpGet]
        [CommerceTracking(TrackingType.Wishlist)]
        public ActionResult Index(WishListPage currentPage)
        {
            var viewModel = _cartViewModelFactory.CreateWishListViewModel(WishList);
            viewModel.CurrentPage = currentPage;

            return View(viewModel);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult WishListMiniCartDetails()
        {
            var viewModel = _cartViewModelFactory.CreateWishListMiniCartViewModel(WishList);
            return PartialView("_WishListMiniCartDetails", viewModel);
        }

        [HttpPost]
        [AllowDBWrite]
        public async Task<ActionResult> AddToCart(string code)
        {
            ModelState.Clear();

            if (WishList == null)
            {
                _wishlist = _cartService.LoadOrCreateCart(_cartService.DefaultWishListName);
            }

            if (WishList.GetAllLineItems().Any(item => item.Code.Equals(code, StringComparison.OrdinalIgnoreCase)))
            {
                return WishListMiniCartDetails();
            }

            var result = _cartService.AddToCart(WishList, code, 1);
            if (result.EntriesAddedToCart)
            {
                _orderRepository.Save(WishList);
                await _recommendationService.TrackWishlistAsync(HttpContext);
                return WishListMiniCartDetails();
            }

            return new HttpStatusCodeResult(500, result.GetComposedValidationMessage());
        }

        [HttpPost]
        [AllowDBWrite]
        public async Task<ActionResult> ChangeCartItem(string code, decimal quantity, string size, string newSize, string displayName)
        {
            ModelState.Clear();

            _cartService.ChangeCartItem(WishList, 0, code, quantity, size, newSize, displayName);
            _orderRepository.Save(WishList);
            await _recommendationService.TrackWishlistAsync(HttpContext);
            return WishListMiniCartDetails();
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult DeleteWishList()
        {
            if (WishList != null)
            {
                _orderRepository.Delete(WishList.OrderLink);
            }
            var startPage = _contentLoader.Get<StartPage>(ContentReference.StartPage);

            return RedirectToAction("Index", new { Node = startPage.WishListPage });
        }

        private ICart WishList
        {
            get { return _wishlist ?? (_wishlist = _cartService.LoadCart(_cartService.DefaultWishListName)); }
        }
    }
}