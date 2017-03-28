using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.ViewModelFactories
{
    [ServiceConfiguration(typeof(CartViewModelFactory), Lifecycle = ServiceInstanceScope.Singleton)]
    public class CartViewModelFactory
    {
        private readonly IContentLoader _contentLoader;
        private readonly ICurrencyService _currencyService;
        private readonly IOrderGroupCalculator _orderGroupCalculator;
        private readonly ShipmentViewModelFactory _shipmentViewModelFactory;
        private readonly ReferenceConverter _referenceConverter;

        public CartViewModelFactory(
            IContentLoader contentLoader,
            ICurrencyService currencyService,
            IOrderGroupCalculator orderGroupCalculator,
            ShipmentViewModelFactory shipmentViewModelFactory,
            ReferenceConverter referenceConverter)
        {
            _contentLoader = contentLoader;
            _currencyService = currencyService;
            _orderGroupCalculator = orderGroupCalculator;
            _shipmentViewModelFactory = shipmentViewModelFactory;
            _referenceConverter = referenceConverter;
        }

        public virtual MiniCartViewModel CreateMiniCartViewModel(ICart cart)
        {
            if (cart == null)
            {
                return new MiniCartViewModel
                {
                    ItemCount = 0,
                    CheckoutPage = _contentLoader.Get<StartPage>(ContentReference.StartPage).CheckoutPage,
                    Shipments = Enumerable.Empty<ShipmentViewModel>(),
                    Total = new Money(0, _currencyService.GetCurrentCurrency())
                };
            }
            return new MiniCartViewModel
            {
                ItemCount = GetLineItemsTotalQuantity(cart),
                CheckoutPage = _contentLoader.Get<StartPage>(ContentReference.StartPage).CheckoutPage,
                Shipments = _shipmentViewModelFactory.CreateShipmentsViewModel(cart),
                Total = _orderGroupCalculator.GetSubTotal(cart)
            };
        }

        public virtual LargeCartViewModel CreateLargeCartViewModel(ICart cart)
        {
            if (cart == null)
            {
                var zeroAmount = new Money(0, _currencyService.GetCurrentCurrency());

                return new LargeCartViewModel
                {
                    Shipments = Enumerable.Empty<ShipmentViewModel>(),
                    TotalDiscount = zeroAmount,
                    Total = zeroAmount
                };
            }

            return new LargeCartViewModel
            {
                Shipments = _shipmentViewModelFactory.CreateShipmentsViewModel(cart),
                TotalDiscount = new Money(cart.GetAllLineItems().Sum(x => x.GetEntryDiscount()), cart.Currency),
                Total = _orderGroupCalculator.GetSubTotal(cart)
            };
        }

        public virtual WishListViewModel CreateWishListViewModel(ICart cart)
        {
            if (cart == null)
            {
                return new WishListViewModel
                {
                    ItemCount = 0,
                    CartItems = new CartItemViewModel[0],
                    Total = new Money(0, _currencyService.GetCurrentCurrency())
                };
            }

            return new WishListViewModel
            {
                ItemCount = GetLineItemsTotalQuantity(cart),
                CartItems = _shipmentViewModelFactory.CreateShipmentsViewModel(cart).SelectMany(x => x.CartItems),
                Total = _orderGroupCalculator.GetSubTotal(cart)
            };
        }

        public virtual WishListMiniCartViewModel CreateWishListMiniCartViewModel(ICart cart)
        {
            if (cart == null)
            {
                return new WishListMiniCartViewModel
                {
                    ItemCount = 0,
                    WishListPage = _contentLoader.Get<StartPage>(ContentReference.StartPage).WishListPage,
                    CartItems = new CartItemViewModel[0],
                    Total = new Money(0, _currencyService.GetCurrentCurrency())
                };
            }

            return new WishListMiniCartViewModel
            {
                ItemCount = GetLineItemsTotalQuantity(cart),
                WishListPage = _contentLoader.Get<StartPage>(ContentReference.StartPage).WishListPage,
                CartItems = _shipmentViewModelFactory.CreateShipmentsViewModel(cart).SelectMany(x => x.CartItems),
                Total = _orderGroupCalculator.GetSubTotal(cart)
            };
        }

        private decimal GetLineItemsTotalQuantity(ICart cart)
        {
            var cartItems = cart
                .GetAllLineItems()
                .Where(c => !ContentReference.IsNullOrEmpty(_referenceConverter.GetContentLink(c.Code)));
            return cartItems.Sum(x => x.Quantity);
        }
    }
}