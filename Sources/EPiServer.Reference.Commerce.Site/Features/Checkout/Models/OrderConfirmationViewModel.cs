using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Registration.Models;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using System;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Models
{
    public class OrderConfirmationViewModel
    {
        public OrderConfirmationPage CurrentPage { get; set; }
        public bool HasOrder { get; set; }
        public OrderConfirmationRegistrationFormModel RegistrationFormModel { get; set; }
        public IEnumerable<LineItem> Items { get; set; }
        public OrderAddress Address { get; set; }
        public CreditCardPayment Payment { get; set; }
        public Guid ContactId { get; set; }
        public DateTime Created { get; set; }
        public int GroupId { get; set; }
        public Money TotalPrice { get; set; }
    }
}