using System.Collections.Generic;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.WishList.Pages;

namespace EPiServer.Reference.Commerce.Site.Features.WishList.Models
{
    public class WishListViewModel
    {
        public WishListPage CurrentPage { get; set; }
        public List<ProductViewModel> Products { get; set; }
    }
}