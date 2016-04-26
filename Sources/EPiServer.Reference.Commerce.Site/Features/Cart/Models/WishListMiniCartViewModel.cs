using EPiServer.Core;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.Models
{
    public class WishListMiniCartViewModel : CartViewModel
    {
        public ContentReference WishListPage { get; set; }
    }
}