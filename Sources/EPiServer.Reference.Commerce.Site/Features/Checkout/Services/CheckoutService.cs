using EPiServer.Reference.Commerce.Site.Features.Payment.Models;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Website;
using Mediachase.Commerce.Website.Helpers;
using Mediachase.MetaDataPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Services
{
    [ServiceConfiguration(typeof(ICheckoutService), Lifecycle = ServiceInstanceScope.Transient)]
    public class CheckoutService : ICheckoutService
    {
        private readonly Func<string, CartHelper> _cartHelper;
        private readonly ICurrentMarket _currentMarket;
        private readonly LanguageService _languageService;
        private readonly CountryManagerFacade _countryManager;

        public CheckoutService(
            Func<string, CartHelper> cartHelper, 
            ICurrentMarket currentMarket, 
            LanguageService languageService, 
            CountryManagerFacade countryManager)
        {
            _cartHelper = cartHelper;
            _currentMarket = currentMarket;
            _languageService = languageService;
            _countryManager = countryManager;
        }

        public Shipment CreateShipment()
        {
            if (CartHelper.Cart.ObjectState == MetaObjectState.Added)
            {
                CartHelper.Cart.AcceptChanges();
            }

            var orderForms = CartHelper.Cart.OrderForms;
            if (orderForms.Count == 0)
            {
                orderForms.AddNew().AcceptChanges();
                orderForms.Single().Name = CartHelper.Cart.Name;
            }

            var orderForm = orderForms.First();

            var shipments = orderForm.Shipments;
            if (shipments.Count != 0)
            {
                shipments.Clear();
            }

            var shipment = shipments.AddNew();
            for (var i = 0; i < orderForm.LineItems.Count; i++)
            {
                var item = orderForm.LineItems[i];
                shipment.AddLineItemIndex(i, item.Quantity);
            }
            shipment.AcceptChanges();

            return shipment;
        }

        public void UpdateShipment(Shipment shipment, ShippingRate shippingCost)
        {
            shipment.ShippingMethodId = shippingCost.Id;
            shipment.ShippingMethodName = shippingCost.Name;
            shipment.SubTotal = shippingCost.Money.Amount;
            shipment.ShippingSubTotal = shippingCost.Money.Amount;
            shipment.AcceptChanges();
        }

        public ShippingRate GetShippingRate(Shipment shipment, Guid shippingMethodId)
        {
            var method = ShippingManager.GetShippingMethod(shippingMethodId).ShippingMethod.Single();
            return GetRate(shipment, method);
        }

        private ShippingRate GetRate(Shipment shipment, ShippingMethodDto.ShippingMethodRow shippingMethodRow)
        {
            var type = Type.GetType(shippingMethodRow.ShippingOptionRow.ClassName);
            var shippingGateway = (IShippingGateway)Activator.CreateInstance(type, _currentMarket.GetCurrentMarket());
            string message = null;
            return shippingGateway.GetRate(shippingMethodRow.ShippingMethodId, shipment, ref message);
        }

        public IEnumerable<ShippingRate> GetShippingRates(Shipment shipment)
        {
            var methods = ShippingManager.GetShippingMethodsByMarket(CurrentMarketId.Value, false).ShippingMethod;
            var currentLanguage = CurrentLanguageIsoCode;
            var currencyId = CartHelper.Cart.BillingCurrency;
            return methods.
                Where(shippingMethodRow =>
                    currentLanguage.Equals(shippingMethodRow.LanguageId, StringComparison.OrdinalIgnoreCase) &&
                    String.Equals(currencyId, shippingMethodRow.Currency, StringComparison.OrdinalIgnoreCase)).
                OrderBy(shippingMethodRow => shippingMethodRow.Ordering).
                Select(shippingMethodRow => GetRate(shipment, shippingMethodRow));
        }

        public IEnumerable<PaymentMethodViewModel<IPaymentOption>> GetPaymentMethods()
        {
            var methods = PaymentManager.GetPaymentMethodsByMarket(CurrentMarketId.Value).PaymentMethod.Where(c => c.IsActive);
            var currentLanguage = CurrentLanguageIsoCode;
            return methods.
                Where(paymentRow => currentLanguage.Equals(paymentRow.LanguageId, StringComparison.OrdinalIgnoreCase)).
                OrderBy(paymentRow => paymentRow.Ordering).
                Select(paymentRow => new PaymentMethodViewModel<IPaymentOption>
                {
                    Id = paymentRow.PaymentMethodId,
                    SystemName = paymentRow.SystemKeyword,
                    FriendlyName = paymentRow.Name,
                    MarketId = CurrentMarketId,
                    Ordering = paymentRow.Ordering,
                    IsDefault = paymentRow.IsDefault,
                    Description = paymentRow.Description,
                }).ToList();
        }

        public void DeleteCart()
        {
            var cart = CartHelper.Cart;
            foreach (OrderForm orderForm in cart.OrderForms)
            {
                foreach (Shipment shipment in orderForm.Shipments)
                {
                    shipment.Delete();
                }
                orderForm.Delete();
            }
            foreach (OrderAddress address in cart.OrderAddresses)
            {
                address.Delete();
            }
            
            CartHelper.Delete();

            cart.AcceptChanges();
        }

        private CartHelper CartHelper
        {
            get { return _cartHelper(Mediachase.Commerce.Orders.Cart.DefaultName); }
        }

        private MarketId CurrentMarketId
        {
            get { return _currentMarket.GetCurrentMarket().MarketId; }
        }

        private string CurrentLanguageIsoCode
        {
            get { return _languageService.GetCurrentLanguage().TwoLetterISOLanguageName; }
        }

        public OrderAddress AddNewOrderAddress()
        {
            return  CartHelper.Cart.OrderAddresses.AddNew();
        }

        public void UpdateBillingAddressId(string addressId)
        {
            CartHelper.Cart.OrderForms[0].BillingAddressId = addressId;
            CartHelper.Cart.OrderForms[0].AcceptChanges();
        }

        public void ClearOrderAddresses()
        {
            CartHelper.Cart.OrderAddresses.Clear();
        }

        public PurchaseOrder SaveCartAsPurchaseOrder()
        {
            return CartHelper.Cart.SaveAsPurchaseOrder();
        }
    }
}