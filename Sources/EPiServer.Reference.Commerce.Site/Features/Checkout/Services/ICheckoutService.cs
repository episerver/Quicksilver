using EPiServer.Reference.Commerce.Site.Features.Cart.Models;
using EPiServer.Reference.Commerce.Site.Features.Payment.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Website;
using System;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Services
{
    public interface ICheckoutService
    {
        IEnumerable<Shipment> CreateShipments(IEnumerable<CartItem> cartItems, IEnumerable<ShippingAddress> shippingAddresses);
        void UpdateShipment(Shipment shipment, ShippingRate shippingCost);
        ShippingRate GetShippingRate(Shipment shipment, Guid shippingMethodId);
        IEnumerable<ShippingRate> GetShippingRates(Shipment shipment);
        IEnumerable<PaymentMethodViewModel<IPaymentOption>> GetPaymentMethods();
        OrderAddress AddNewOrderAddress();
        void UpdateBillingAddressId(string addressId);
        void ClearOrderAddresses();
        PurchaseOrder SaveCartAsPurchaseOrder();
        void DeleteCart();
    }
}