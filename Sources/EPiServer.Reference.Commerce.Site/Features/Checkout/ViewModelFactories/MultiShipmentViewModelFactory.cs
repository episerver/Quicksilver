using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModelFactories
{
    [ServiceConfiguration(typeof(MultiShipmentViewModelFactory), Lifecycle = ServiceInstanceScope.Singleton)]
    public class MultiShipmentViewModelFactory
    {
        readonly LocalizationService _localizationService;
        readonly IAddressBookService _addressBookService;
        readonly IContentLoader _contentLoader;
        readonly UrlResolver _urlResolver;
        readonly ServiceAccessor<HttpContextBase> _httpContextAccessor;
        readonly ShipmentViewModelFactory _shipmentViewModelFactory;

        public MultiShipmentViewModelFactory(
            LocalizationService localizationService,
            IAddressBookService addressBookService,
            IContentLoader contentLoader,
            UrlResolver urlResolver,
            ServiceAccessor<HttpContextBase> httpContextAccessor,
            ShipmentViewModelFactory shipmentViewModelFactory)
        {
            _localizationService = localizationService;
            _addressBookService = addressBookService;
            _contentLoader = contentLoader;
            _urlResolver = urlResolver;
            _httpContextAccessor = httpContextAccessor;
            _shipmentViewModelFactory = shipmentViewModelFactory;
        }

        public virtual MultiShipmentViewModel CreateMultiShipmentViewModel(ICart cart, bool isAuthenticated)
        {
            var viewModel = new MultiShipmentViewModel
            {
                StartPage = _contentLoader.Get<StartPage>(ContentReference.StartPage),
                AvailableAddresses = GetAvailableShippingAddresses(),
                CartItems = cart != null ? FlattenCartItems(_shipmentViewModelFactory.CreateShipmentsViewModel(cart).SelectMany(x => x.CartItems)) : new CartItemViewModel[0],
                ReferrerUrl = GetReferrerUrl()
            };

            if (!isAuthenticated)
            {
                UpdateShippingAddressesForAnonymous(viewModel);
            }

            return viewModel;
        }

        private IList<AddressModel> GetAvailableShippingAddresses()
        {
            var addresses = _addressBookService.List();
            foreach (var address in addresses.Where(x => string.IsNullOrEmpty(x.Name)))
            {
                address.Name = _localizationService.GetString("/Shared/Address/DefaultAddressName");
            }

            return addresses;
        }

        private void UpdateShippingAddressesForAnonymous(MultiShipmentViewModel viewModel)
        {
            foreach (var item in viewModel.CartItems)
            {
                var anonymousShippingAddress = new AddressModel
                {
                    AddressId = Guid.NewGuid().ToString(),
                    Name = Guid.NewGuid().ToString(),
                    CountryCode = "USA"
                };

                item.AddressId = anonymousShippingAddress.AddressId;
                _addressBookService.LoadCountriesAndRegionsForAddress(anonymousShippingAddress);
                viewModel.AvailableAddresses.Add(anonymousShippingAddress);
            }
        }

        private string GetReferrerUrl()
        {
            var httpContext = _httpContextAccessor();
            if (httpContext.Request.UrlReferrer != null &&
                httpContext.Request.UrlReferrer.Host.Equals(httpContext.Request.Url.Host, StringComparison.OrdinalIgnoreCase))
            {
                return httpContext.Request.UrlReferrer.ToString();
            }
            return _urlResolver.GetUrl(ContentReference.StartPage);
        }

        private CartItemViewModel [] FlattenCartItems(IEnumerable<CartItemViewModel> cartItems)
        {
            var list = new List<CartItemViewModel>();

            foreach (var item in cartItems)
            {
                for (int i = 0; i < item.Quantity; i++)
                {
                    list.Add(new CartItemViewModel
                    {
                        Quantity = 1,
                        AvailableSizes = item.AvailableSizes,
                        Brand = item.Brand,
                        DisplayName = item.DisplayName,
                        Code = item.Code,
                        ImageUrl = item.ImageUrl,
                        IsAvailable = item.IsAvailable,
                        PlacedPrice = item.PlacedPrice,
                        AddressId = item.AddressId,
                        Url = item.Url,
                        Entry = item.Entry,
                        DiscountedUnitPrice = item.DiscountedUnitPrice,
                        DiscountedPrice = item.DiscountedPrice,
                        IsGift = item.IsGift
                    });
                }
            }

            return list.ToArray();
        }
    }
}