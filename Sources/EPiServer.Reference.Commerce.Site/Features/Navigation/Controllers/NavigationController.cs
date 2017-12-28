using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Navigation.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.SpecializedProperties;
using EPiServer.Web.Mvc.Html;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Navigation.Controllers
{
    public class NavigationController : Controller
    {
        private readonly IContentLoader _contentLoader;
        private readonly ICartService _cartService;
        private readonly UrlHelper _urlHelper;
        private readonly LocalizationService _localizationService;
        readonly CartViewModelFactory _cartViewModelFactory;

        public NavigationController(
            IContentLoader contentLoader, 
            ICartService cartService, 
            UrlHelper urlHelper, 
            LocalizationService localizationService,
            CartViewModelFactory cartViewModelFactory)
        {
            _contentLoader = contentLoader;
            _cartService = cartService;
            _urlHelper = urlHelper;
            _localizationService = localizationService;
            _cartViewModelFactory = cartViewModelFactory;
        }

        public ActionResult Index(IContent currentContent)
        {
            var cart = _cartService.LoadCart(_cartService.DefaultCartName);

            var wishlist = _cartService.LoadCart(_cartService.DefaultWishListName);
            var startPage = _contentLoader.Get<StartPage>(ContentReference.StartPage);
           
            var viewModel = new NavigationViewModel
            {
                StartPage = startPage,
                CurrentContentLink = currentContent?.ContentLink,
                UserLinks = new LinkItemCollection(),
                MiniCart = _cartViewModelFactory.CreateMiniCartViewModel(cart),
                WishListMiniCart = _cartViewModelFactory.CreateWishListMiniCartViewModel(wishlist)
            };

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                var rightMenuItems = startPage.RightMenu;
                if (rightMenuItems != null)
                {
                    viewModel.UserLinks.AddRange(rightMenuItems);
                }
                
                viewModel.UserLinks.Add(new LinkItem 
                {
                    Href = _urlHelper.Action("SignOut", "Login"), 
                    Text = _localizationService.GetString("/Header/Account/SignOut") 
                });
            }
            else
            {
                viewModel.UserLinks.Add(new LinkItem 
                { 
                    Href = _urlHelper.Action("Index", "Login",  new { returnUrl = currentContent != null ? _urlHelper.ContentUrl(currentContent.ContentLink) : "/" }), 
                    Text = _localizationService.GetString("/Header/Account/SignIn") 
                });
            }

            return PartialView(viewModel);
        }
    }
}