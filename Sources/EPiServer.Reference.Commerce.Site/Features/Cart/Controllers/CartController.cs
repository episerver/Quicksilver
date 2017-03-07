using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private ICart _cart;
        private readonly IOrderRepository _orderRepository;
        private readonly IRecommendationService _recommendationService;
        readonly CartViewModelFactory _cartViewModelFactory;
        

        public CartController(
            ICartService cartService,
            IOrderRepository orderRepository,
            IRecommendationService recommendationService,
            CartViewModelFactory cartViewModelFactory)
        {
            _cartService = cartService;
            _orderRepository = orderRepository;
            _recommendationService = recommendationService;
            _cartViewModelFactory = cartViewModelFactory;
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult MiniCartDetails()
        {
            var viewModel = _cartViewModelFactory.CreateMiniCartViewModel(Cart);
            return PartialView("_MiniCartDetails", viewModel);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult LargeCart()
        {
            var viewModel = _cartViewModelFactory.CreateLargeCartViewModel(Cart);
            return PartialView("LargeCart", viewModel);
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult AddToCart(string code)
        {
            string warningMessage = string.Empty;

            ModelState.Clear();

            if (Cart == null)
            {
                _cart = _cartService.LoadOrCreateCart(_cartService.DefaultCartName);
            }

            var result = _cartService.AddToCart(Cart, code, 1);
            if (result.EntriesAddedToCart)
            {
                _orderRepository.Save(Cart);
                _recommendationService.SendCartTrackingData(HttpContext);
                return MiniCartDetails();
            }
            
            return new HttpStatusCodeResult(500, result.GetComposedValidationMessage());
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult ChangeCartItem(int shipmentId, string code, decimal quantity, string size, string newSize)
        {
            ModelState.Clear();

            _cartService.ChangeCartItem(Cart, shipmentId, code, quantity, size, newSize);
            _orderRepository.Save(Cart);
            _recommendationService.SendCartTrackingData(HttpContext);
            return MiniCartDetails();
        }

        private ICart Cart
        {
            get { return _cart ?? (_cart = _cartService.LoadCart(_cartService.DefaultCartName)); }
        }
    }
}