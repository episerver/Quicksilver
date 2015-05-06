using System;
using System.Collections.Generic;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Payment.Models;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Models
{
    public class CheckoutViewModel
    {
        public StartPage StartPage { get; set; }
        public CheckoutPage CurrentPage { get; set; }
        public IEnumerable<PaymentMethodViewModel> PaymentMethodViewModels { get; set; }
        public IEnumerable<ShippingMethodViewModel> ShippingMethodViewModels { get; set; }
        public CheckoutFormModel CheckoutFormModel { get; set; }
        public string SelectedPaymentMethodSystemName { get; set; }
        public string ReferrerUrl { get; set; }
        public Dictionary<Guid, string> Addresses { get; set; }
    }
}