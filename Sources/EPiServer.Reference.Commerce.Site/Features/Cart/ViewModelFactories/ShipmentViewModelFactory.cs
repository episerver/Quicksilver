using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Services;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Cart.ViewModelFactories
{
    [ServiceConfiguration(typeof(ShipmentViewModelFactory), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ShipmentViewModelFactory
    {
        private readonly CatalogContentService _catalogContentService;
        private readonly ShippingManagerFacade _shippingManagerFacade;
        private readonly LanguageService _languageService;
        private readonly IAddressBookService _addressBookService;
        readonly CartItemViewModelFactory _cartItemViewModelFactory;
        readonly IMarketService _marketService;

        public ShipmentViewModelFactory(
            CatalogContentService catalogContentService,
            ShippingManagerFacade shippingManagerFacade,
            LanguageService languageService,
            IAddressBookService addressBookService,
            CartItemViewModelFactory cartItemViewModelFactory,
            IMarketService marketService)
        {
            _catalogContentService = catalogContentService;
            _shippingManagerFacade = shippingManagerFacade;
            _languageService = languageService;
            _addressBookService = addressBookService;
            _cartItemViewModelFactory = cartItemViewModelFactory;
            _marketService = marketService;
        }

        public virtual IEnumerable<ShipmentViewModel> CreateShipmentsViewModel(ICart cart)
        {
            foreach (var shipment in cart.GetFirstForm().Shipments)
            {
                var shipmentModel = new ShipmentViewModel
                {
                    ShipmentId = shipment.ShipmentId,
                    CartItems = new List<CartItemViewModel>(),
                    Address = _addressBookService.ConvertToModel(shipment.ShippingAddress),
                    ShippingMethods = CreateShippingMethodViewModels(cart.MarketId, cart.Currency, shipment)
                };

                shipmentModel.ShippingMethodId = shipment.ShippingMethodId == Guid.Empty && shipmentModel.ShippingMethods.Any() ? 
                                                 shipmentModel.ShippingMethods.First().Id 
                                               : shipment.ShippingMethodId;

                var entries = _catalogContentService.GetItems<EntryContentBase>(shipment.LineItems.Select(x => x.Code));

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

        private IEnumerable<ShippingMethodViewModel> CreateShippingMethodViewModels(MarketId marketId, Currency currency, IShipment shipment)
        {
            var market = _marketService.GetMarket(marketId);
            var shippingRates = GetShippingRates(market, currency, shipment);
            return shippingRates.Any()
                ? shippingRates.Select(r => new ShippingMethodViewModel { Id = r.Id, DisplayName = r.Name, Price = r.Money })
                : Enumerable.Empty<ShippingMethodViewModel>();
        }

        private IEnumerable<ShippingRate> GetShippingRates(IMarket market, Currency currency, IShipment shipment)
        {
            var methods = _shippingManagerFacade.GetShippingMethodsByMarket(market.MarketId.Value, false);
            var currentLanguage = _languageService.GetCurrentLanguage().TwoLetterISOLanguageName;

            return methods.Where(shippingMethodRow => currentLanguage.Equals(shippingMethodRow.LanguageId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(currency, shippingMethodRow.Currency, StringComparison.OrdinalIgnoreCase))
                .OrderBy(shippingMethodRow => shippingMethodRow.Ordering)
                .Select(shippingMethodRow => _shippingManagerFacade.GetRate(shipment, shippingMethodRow, market))
                .Where(rate => rate != null);
        }
    }
}