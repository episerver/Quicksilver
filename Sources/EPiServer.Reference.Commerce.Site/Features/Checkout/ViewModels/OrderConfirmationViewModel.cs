using System;
using System.Collections.Generic;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.ViewModels;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels
{
    public class OrderConfirmationViewModel<T> : PageViewModel<T> where T : PageData
    {
        public bool HasOrder { get; set; }
        public string OrderId { get; set; }
        public AddressModel BillingAddress { get; set; }
        public IEnumerable<IPayment> Payments { get; set; }
        public Guid ContactId { get; set; }
        public DateTime Created { get; set; }
        public int OrderGroupId { get; set; }
        public string NotificationMessage { get; set; }
        public Currency Currency { get; set; }
        public Money HandlingTotal { get; set; }
        public Money ShippingSubTotal { get; set; }
        public Money ShippingDiscountTotal { get; set; }
        public Money ShippingTotal { get; set; }
        public Money TaxTotal { get; set; }
        public Money CartTotal { get; set; }
        public Money OrderLevelDiscountTotal { get; set; }
        public IEnumerable<ShipmentConfirmationViewModel> Shipments { get; set; }
    }
}