using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private ICart _cart;
        private readonly IOrderRepository _orderRepository;
        readonly CartViewModelFactory _cartViewModelFactory;

        public CartController(
            ICartService cartService,
            IOrderRepository orderRepository,
            CartViewModelFactory cartViewModelFactory)
        {
            _cartService = cartService;
            _orderRepository = orderRepository;
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

            if (_cartService.AddToCart(Cart, code, out warningMessage))
            {
                _orderRepository.Save(Cart);
                return MiniCartDetails();
            }

            // HttpStatusMessage can't be longer than 512 characters.
            warningMessage = warningMessage.Length < 512 ? warningMessage : warningMessage.Substring(512);

            return new HttpStatusCodeResult(500, warningMessage);
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult ChangeCartItem(int shipmentId, string code, decimal quantity, string size, string newSize)
        {
            ModelState.Clear();

            _cartService.ChangeCartItem(Cart, shipmentId, code, quantity, size, newSize);
            _orderRepository.Save(Cart);
            return MiniCartDetails();
        }

        private ICart Cart
        {
            get { return _cart ?? (_cart = _cartService.LoadCart(_cartService.DefaultCartName)); }
        }
    }
}