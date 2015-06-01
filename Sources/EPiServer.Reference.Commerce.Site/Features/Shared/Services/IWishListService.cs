using EPiServer.Reference.Commerce.Site.Features.WishList.Models;
using EPiServer.Reference.Commerce.Site.Features.WishList.Pages;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Services
{
    public interface IWishListService
    {
        WishListViewModel GetViewModel(WishListPage currentPage);
        bool AddItem(string code, out string warningMessage);
        void RemoveItem(string code);
        void Delete();
    }
}
