using System;
using EPiServer.Reference.Commerce.Site.Features.Payment.Models;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using Mediachase.Commerce.Website;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Models
{
    public class CheckoutFormModel
    {
        [LocalizedRequired("/Checkout/Shipment/Empty/ChooseDelivery")]
        public Guid SelectedShippingMethodId { get; set; }

        [LocalizedRequired("/Checkout/Payment/Empty/ChoosePayment")]
        public Guid SelectedPaymentMethodId { get; set; }

        public AddressFormModel AddressFormModel { get; set; }
        public IPaymentMethodViewModel<IPaymentOption> PaymentViewModel { get; set; }
    }
}