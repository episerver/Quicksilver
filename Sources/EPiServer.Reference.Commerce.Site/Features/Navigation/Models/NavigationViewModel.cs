using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Cart.Models;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.SpecializedProperties;
using Mediachase.Commerce;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Navigation.Models
{
    public class NavigationViewModel
    {
        public ContentReference CurrentContentLink { get; set; }
        public StartPage StartPage { get; set; }
        public LinkItemCollection UserLinks { get; set; }
        public MiniCartViewModel MiniCart { get; set; }
        public WishListMiniCartViewModel WishListMiniCart { get; set; }
    }
}