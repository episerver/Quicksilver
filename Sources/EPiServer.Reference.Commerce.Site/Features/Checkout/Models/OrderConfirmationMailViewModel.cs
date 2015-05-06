using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using Mediachase.Commerce.Orders;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Models
{
    public class OrderConfirmationMailViewModel
    {
        public OrderConfirmationMailPage CurrentPage { get; set; }
        public PurchaseOrder Order { get; set; }
    }
}