using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Globalization;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Orders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.ViewModelFactories
{
    [ServiceConfiguration(typeof(ShipmentViewModelFactory), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ShipmentViewModelFactory
    {
        private readonly IContentLoader _contentLoader;
        private readonly ShippingManagerFacade _shippingManagerFacade;
        private readonly LanguageService _languageService;
        private readonly ReferenceConverter _referenceConverter;
        private readonly IAddressBookService _addressBookService;
        readonly CartItemViewModelFactory _cartItemViewModelFactory;
        private readonly LanguageResolver _languageResolver;

        public ShipmentViewModelFactory(
            IContentLoader contentLoader,
            ShippingManagerFacade shippingManagerFacade,
            LanguageService languageService,
            ReferenceConverter referenceConverter,
            IAddressBookService addressBookService,
            CartItemViewModelFactory cartItemViewModelFactory,
            LanguageResolver languageResolver)
        {
            _contentLoader = contentLoader;
            _shippingManagerFacade = shippingManagerFacade;
            _languageService = languageService;
            _referenceConverter = referenceConverter;
            _addressBookService = addressBookService;
            _cartItemViewModelFactory = cartItemViewModelFactory;
            _languageResolver = languageResolver;
        }

        public virtual IEnumerable<ShipmentViewModel> CreateShipmentsViewModel(ICart cart)
        {
            var preferredCulture = _languageResolver.GetPreferredCulture();
            foreach (var shipment in cart.GetFirstForm().Shipments)
            {
                var shipmentModel = new ShipmentViewModel
                {
                    ShipmentId = shipment.ShipmentId,
                    CartItems = new List<CartItemViewModel>(),
                    Address = _addressBookService.ConvertToModel(shipment.ShippingAddress),
                    ShippingMethods = CreateShippingMethodViewModels(cart.Market, cart.Currency, shipment)
                };

                shipmentModel.ShippingMethodId = shipment.ShippingMethodId == Guid.Empty && shipmentModel.ShippingMethods.Any() ? 
                                                 shipmentModel.ShippingMethods.First().Id 
                                               : shipment.ShippingMethodId;

                var entries = _contentLoader.GetItems(shipment.LineItems.Select(x => _referenceConverter.GetContentLink(x.Code)),
                    preferredCulture).OfType<EntryContentBase>();

                foreach (var lineItem in shipment.LineItems)
                {
                    var entry = entries.FirstOrDefault(x => x.Code == lineItem.Code);
                    if (entry == null)
                    {
                        //Entry was deleted, skip processing.
                        continue;
                    }

                    shipmentModel.CartItems.Add(_cartItemViewModelFactory.CreateCartItemViewModel(cart, lineItem, entry));
                }

                yield return shipmentModel;
            }
        }

        private IEnumerable<ShippingMethodViewModel> CreateShippingMethodViewModels(IMarket market, Currency currency, IShipment shipment)
        {
            var shippingRates = GetShippingRates(market, currency, shipment);
            return shippingRates.Select(r => new ShippingMethodViewModel { Id = r.Id, DisplayName = r.Name, Price = r.Money });
        }

        private IEnumerable<ShippingRate> GetShippingRates(IMarket market, Currency currency, IShipment shipment)
        {
            var methods = _shippingManagerFacade.GetShippingMethodsByMarket(market.MarketId.Value, false);
            var currentLanguage = _languageService.GetCurrentLanguage().TwoLetterISOLanguageName;

            return methods.Where(shippingMethodRow => currentLanguage.Equals(shippingMethodRow.LanguageId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(currency, shippingMethodRow.Currency, StringComparison.OrdinalIgnoreCase))
                .OrderBy(shippingMethodRow => shippingMethodRow.Ordering)
                .Select(shippingMethodRow => _shippingManagerFacade.GetRate(shipment, shippingMethodRow, market));
        }
    }
}