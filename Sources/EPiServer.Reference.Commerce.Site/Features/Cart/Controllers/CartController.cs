using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
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
        public async Task<ActionResult> AddToCart(string code)
        {
            ModelState.Clear();

            if (Cart == null)
            {
                _cart = _cartService.LoadOrCreateCart(_cartService.DefaultCartName);
            }

            var result = _cartService.AddToCart(Cart, code, 1);
            if (result.EntriesAddedToCart)
            {
                _orderRepository.Save(Cart);
                await _recommendationService.TrackCartAsync(HttpContext);
                return MiniCartDetails();
            }
            
            return new HttpStatusCodeResult(500, result.GetComposedValidationMessage());
        }

        [HttpPost]
        [AllowDBWrite]
        public async Task<ActionResult> ChangeCartItem(int shipmentId, string code, decimal quantity, string size, string newSize, string displayName)
        {
            ModelState.Clear();

            _cartService.ChangeCartItem(Cart, shipmentId, code, quantity, size, newSize, displayName);
            _orderRepository.Save(Cart);
            await _recommendationService.TrackCartAsync(HttpContext);
            return MiniCartDetails();
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult UpdateShippingMethod(UpdateShippingMethodViewModel viewModel)
        {
            ModelState.Clear();

            _cartService.UpdateShippingMethod(Cart, viewModel.ShipmentId, viewModel.ShippingMethodId);
            _orderRepository.Save(Cart);

            return LargeCart();
        }

        private ICart Cart
        {
            get { return _cart ?? (_cart = _cartService.LoadCart(_cartService.DefaultCartName)); }
        }
    }
}