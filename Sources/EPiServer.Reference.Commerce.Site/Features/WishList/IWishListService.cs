using EPiServer.Reference.Commerce.Site.Features.WishList.Models;
using EPiServer.Reference.Commerce.Site.Features.WishList.Pages;

namespace EPiServer.Reference.Commerce.Site.Features.WishList
{
    public interface IWishListService
    {
        WishListViewModel GetViewModel(WishListPage currentPage);
        void AddItem(string code);
        void RemoveItem(string code);
        void Delete();
    }
}
