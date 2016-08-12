using EPiServer.Reference.Commerce.Site.Features.Cart.Models;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Payment.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using Mediachase.Commerce.Website;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Models
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
        public IEnumerable<PaymentMethodViewModel<IPaymentOption>> PaymentMethodViewModels { get; set; }

        /// <summary>
        /// Gets ors sets all available shipping method that can be associated to a shipping address.
        /// </summary>
        public IEnumerable<ShippingMethodViewModel> ShippingMethodViewModels { get; set; }

        public string ReferrerUrl { get; set; }

        /// <summary>
        /// Gets a list of all existing addresses for the current customer and that can be used for billing and shipment.
        /// </summary>
        public IList<ShippingAddress> AvailableAddresses { get; set; }

        /// <summary>
        /// Gets or sets the billing address.
        /// </summary>
        public ShippingAddress BillingAddress { get; set; }

        /// <summary>
        /// Gets or sets all shipping addresses for the current purchase.
        /// </summary>
        public IList<ShippingAddress> ShippingAddresses { get; set; }

        /// <summary>
        /// Gets or sets the payment method associated to the current purchase.
        /// </summary>
        public IPaymentMethodViewModel<IPaymentOption> Payment { get; set; }

        /// <summary>
        /// Gets or sets whether the shipping address should be the same as the billing address.
        /// </summary>
        public bool UseBillingAddressForShipment { get; set; }

        /// <summary>
        /// Gets or sets all <see cref="CartItem"/>s in the current cart.
        /// </summary>
        public CartItem[] CartItems { get; set; }

        /// <summary>
        /// Gets the name of the checkout view required depending on the number of distinct shipping addresses.
        /// </summary>
        public string ViewName
        {
            get
            {
                return CartItems != null && CartItems
                    .Where(x => x.AddressId.HasValue)
                    .Select(x => x.AddressId)
                    .GroupBy(x => x)
                    .Count() > 1 ? MultiShipmentCheckoutViewName : SingleShipmentCheckoutViewName;
            }
        }
    }
}