using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.ViewModelFactories
{
    [ServiceConfiguration(typeof(CartViewModelFactory), Lifecycle = ServiceInstanceScope.Singleton)]
    public class CartViewModelFactory
    {
        private readonly IContentLoader _contentLoader;
        private readonly IPricingService _pricingService;
        private readonly IOrderGroupCalculator _orderGroupCalculator;
        private readonly ShipmentViewModelFactory _shipmentViewModelFactory;
        private readonly ReferenceConverter _referenceConverter;

        public CartViewModelFactory(
            IContentLoader contentLoader,
            IPricingService pricingService,
            IOrderGroupCalculator orderGroupCalculator,
            ShipmentViewModelFactory shipmentViewModelFactory,
            ReferenceConverter referenceConverter)
        {
            _contentLoader = contentLoader;
            _pricingService = pricingService;
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
                    Total = _pricingService.GetMoney(0)
                };
            }
            return new MiniCartViewModel
            {
                ItemCount = GetCartLineItems(cart).Sum(x => x.Quantity),
                CheckoutPage = _contentLoader.Get<StartPage>(ContentReference.StartPage).CheckoutPage,
                Shipments = _shipmentViewModelFactory.CreateShipmentsViewModel(cart),
                Total = _orderGroupCalculator.GetSubTotal(cart)
            };
        }

        public virtual LargeCartViewModel CreateLargeCartViewModel(ICart cart)
        {
            if (cart == null)
            {
                var zeroAmount = _pricingService.GetMoney(0);

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
                    Total = _pricingService.GetMoney(0)
                };
            }

            return new WishListViewModel
            {
                ItemCount = GetCartLineItems(cart).Count(),
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
                    Total = _pricingService.GetMoney(0)
                };
            }

            return new WishListMiniCartViewModel
            {
                ItemCount = GetCartLineItems(cart).Count(),
                WishListPage = _contentLoader.Get<StartPage>(ContentReference.StartPage).WishListPage,
                CartItems = _shipmentViewModelFactory.CreateShipmentsViewModel(cart).SelectMany(x => x.CartItems),
                Total = _orderGroupCalculator.GetSubTotal(cart)
            };
        }

        private IEnumerable<ILineItem> GetCartLineItems(ICart cart)
        {
            return cart
                .GetAllLineItems()
                .Where(c => !ContentReference.IsNullOrEmpty(_referenceConverter.GetContentLink(c.Code)));
        }
    }
}