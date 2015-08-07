using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.Cart;
using EPiServer.Reference.Commerce.Site.Features.Navigation.Models;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.SpecializedProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPiServer.Web.Mvc.Html;
using EPiServer.Reference.Commerce.Site.Features.Cart.Models;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;

namespace EPiServer.Reference.Commerce.Site.Features.Navigation.Controllers
{
    public class NavigationController : Controller
    {
        private readonly IContentLoader _contentLoader;
        private readonly ICartService _cartService;
        private readonly ICartService _wishListService;
        private readonly UrlHelper _urlHelper;
        private readonly LocalizationService _localizationService;

        public NavigationController(IContentLoader contentLoader, ICartService cartService, ICartService wishListService, UrlHelper urlHelper, LocalizationService localizationService)
        {
            _contentLoader = contentLoader;
            _cartService = cartService;
            _wishListService = wishListService;
            _urlHelper = urlHelper;
            _localizationService = localizationService;

            _wishListService.InitializeAsWishList();
        }

        public ActionResult Index(IContent currentContent)
        {
            StartPage startPage = _contentLoader.Get<StartPage>(ContentReference.StartPage);
            var viewModel = new NavigationViewModel
            {
                StartPage = startPage,
                CurrentContentLink = currentContent != null ? currentContent.ContentLink : null,
                UserLinks = new LinkItemCollection(),
                MiniCart = new MiniCartViewModel
                {
                    ItemCount = _cartService.GetLineItemsTotalQuantity(),
                    CheckoutPage = startPage.CheckoutPage,
                    CartItems = _cartService.GetCartItems(),
                    Total = _cartService.GetSubTotal()
                },
                WishListMiniCart = new WishListMiniCartViewModel
                {
                    ItemCount = _wishListService.GetLineItemsTotalQuantity(),
                    WishListPage = startPage.WishListPage,
                    CartItems = _wishListService.GetCartItems(),
                    Total = _wishListService.GetSubTotal()
                }
            };

            if (Request.LogonUserIdentity.IsAuthenticated)
            {
                var rightMenuItems = _contentLoader.Get<StartPage>(ContentReference.StartPage).RightMenu;
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