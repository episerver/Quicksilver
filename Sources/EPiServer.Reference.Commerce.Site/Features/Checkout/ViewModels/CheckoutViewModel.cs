using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;
using EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels
{
    [Bind(Exclude = "Payment")]
    public class CheckoutViewModel
    {
        public const string MultiShipmentCheckoutViewName = "MultiShipmentCheckout";

        public const string SingleShipmentCheckoutViewName = "SingleShipmentCheckout";

        public StartPage StartPage { get; set; }

        public CheckoutPage CurrentPage { get; set; }

        /// <summary>
        /// Gets or sets a collection of all coupon codes that have been applied.
        /// </summary>
        public IEnumerable<string> AppliedCouponCodes { get; set; }

        /// <summary>
        /// Gets or sets all available payment methods that the customer can choose from.
        /// </summary>
        public IEnumerable<PaymentMethodViewModel<PaymentMethodBase>> PaymentMethodViewModels { get; set; }

        public string ReferrerUrl { get; set; }

        /// <summary>
        /// Gets or sets all existing shipments related to the current order.
        /// </summary>
        public IList<ShipmentViewModel> Shipments { get; set; }

        /// <summary>
        /// Gets or sets a list of all existing addresses for the current customer and that can be used for billing and shipment.
        /// </summary>
        public IList<AddressModel> AvailableAddresses { get; set; }

        /// <summary>
        /// Gets or sets the billing address.
        /// </summary>
        public AddressModel BillingAddress { get; set; }

        /// <summary>
        /// Gets or sets the payment method associated to the current purchase.
        /// </summary>
        public IPaymentMethodViewModel<PaymentMethodBase> Payment { get; set; }

        /// <summary>
        /// Gets or sets whether the shipping address should be the same as the billing address.
        /// </summary>
        public bool UseBillingAddressForShipment { get; set; }

        /// <summary>
        /// Gets the name of the checkout view required depending on the number of distinct shipping addresses.
        /// </summary>
        public string ViewName
        {
            get
            {
                return Shipments.Count() > 1 ? MultiShipmentCheckoutViewName : SingleShipmentCheckoutViewName;
            }
        }
    }
}