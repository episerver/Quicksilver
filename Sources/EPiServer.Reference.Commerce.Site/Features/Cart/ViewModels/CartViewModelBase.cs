using System.Collections.Generic;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels
{
    public abstract class CartViewModelBase
    {
        public decimal ItemCount { get; set; }

        public IEnumerable<CartItemViewModel> CartItems { get; set; }

        public Money Total { get; set; }
    }
}