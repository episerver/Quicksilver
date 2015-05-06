using System;
using System.Collections.Generic;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Models;
using EPiServer.Reference.Commerce.Site.Features.Payment.Models;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout
{
    public interface ICheckoutService
    {
        Shipment CreateShipment();
        void UpdateShipment(Shipment shipment, ShippingRate shippingCost);
        ShippingRate GetShippingRate(Shipment shipment, Guid shippingMethodId);
        IEnumerable<ShippingRate> GetShippingRates(Shipment shipment);
        IEnumerable<PaymentMethodViewModel> GetPaymentMethods();
        AddressFormModel MapAddressToAddressForm(AddressEntity preferredShippingAddress);
        void DeleteCart();
    }
}