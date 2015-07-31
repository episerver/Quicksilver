using EPiServer.Reference.Commerce.Site.Features.Payment.Models;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Website;
using System;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Services
{
    public interface ICheckoutService
    {
        Shipment CreateShipment();
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