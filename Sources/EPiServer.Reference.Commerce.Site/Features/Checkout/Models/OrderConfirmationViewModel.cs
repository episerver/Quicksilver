using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Registration.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.ViewModels;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using System;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Models
{
    public class OrderConfirmationViewModel<T> : PageViewModel<T> where T : PageData
    {
        public bool HasOrder { get; set; }
        public string OrderId { get; set; }
        public OrderConfirmationRegistrationFormModel RegistrationFormModel { get; set; }
        public IEnumerable<LineItem> Items { get; set; }
        public Address BillingAddress { get; set; }
        public IList<Address> ShippingAddresses { get; set; }
        public IEnumerable<Mediachase.Commerce.Orders.Payment> Payments { get; set; }
        public Guid ContactId { get; set; }
        public DateTime Created { get; set; }
        public int GroupId { get; set; }
        public Money ShippingTotal { get; set; }
        public Money TotalPrice { get; set; }
        public string NotificationMessage { get; set; }
        public Dictionary<int, decimal> ItemPrices { get; set; }
    }
}