using Mediachase.Commerce.Security;
using Mediachase.Commerce.Website.Helpers;
using StructureMap.Configuration.DSL;

namespace EPiServer.Reference.Commerce.Site.Features.Cart
{
    public class CartRegistry : Registry
    {
        public CartRegistry()
        {
            // We use this construct to enable delayed instansiation of CartHelper to avoid creating a cart unnecessesarily (CartHelper's ctor)
            For<CartHelper>()
                .Transient()
                .Use(() => new CartHelper(Mediachase.Commerce.Orders.Cart.DefaultName, Security.PrincipalInfo.CurrentPrincipal.GetContactId()));
        }
    }
}