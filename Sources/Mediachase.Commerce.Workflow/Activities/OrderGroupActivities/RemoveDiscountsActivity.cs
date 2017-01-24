using Mediachase.Commerce.Marketing;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.WorkflowCompatibility;
using System.Collections.Generic;

namespace Mediachase.Commerce.Workflow
{
    public class RemoveDiscountsActivity : OrderGroupActivityBase
    {
        /// <summary>
        /// Called by the workflow runtime to execute an activity.
        /// </summary>
        /// <param name="executionContext">The <see cref="T:Mediachase.Commerce.WorkflowCompatibility.ActivityExecutionContext"/> to associate with this <see cref="T:Mediachase.Commerce.WorkflowCompatibility.Activity"/> and execution.</param>
        /// <returns>
        /// The <see cref="T:Mediachase.Commerce.WorkflowCompatibility.ActivityExecutionStatus"/> of the run task, which determines whether the activity remains in the executing state, or transitions to the closed state.
        /// </returns>
        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            // Validate the properties at runtime
            ValidateRuntime();

            // Remove discounts
            RemoveDiscounts();

            // Retun the closed status indicating that this activity is complete.
            return ActivityExecutionStatus.Closed;
        }

        /// <summary>
        /// Removes the discounts.
        /// </summary>
        private void RemoveDiscounts()
        {
            //preserve the coupons in the promotion context so that they can be reapplied duringthe calculate discounts activity 
            List<string> coupons = new List<string>();

            // Process line item discounts first
            foreach (OrderForm form in OrderGroup.OrderForms)
            {
                foreach (LineItem lineItem in form.LineItems)
                {
                    // First remove items
                    foreach (LineItemDiscount discount in lineItem.Discounts)
                    {
                        if (!discount.DiscountName.StartsWith("@") /*&& discount.DiscountId == -1*/) // ignore custom entries
                        {
                            if (!string.IsNullOrEmpty(discount.DiscountCode))
                                coupons.Add(discount.DiscountCode);

                            discount.Delete();
                        }
                    }
                }
            }

            // Process shipment discounts
            foreach (OrderForm form in OrderGroup.OrderForms)
            {
                foreach (Shipment shipment in form.Shipments)
                {
                    foreach (ShipmentDiscount discount in shipment.Discounts)
                    {
                        if (!discount.DiscountName.StartsWith("@") /*&& discount.DiscountId == -1*/) // ignore custom entries
                        {
                            if (!string.IsNullOrEmpty(discount.DiscountCode))
                                coupons.Add(discount.DiscountCode);

                            shipment.ShippingDiscountAmount -= discount.DiscountValue;
                            // Pending testing, this might need to be changed to summing ShippingDiscountAmount before removing them,
                            //  and then adding that sum back to form.ShippingTotal (since CalculateTotalsActivity simply does
                            //  shipment.ShipmentTotal - shipment.ShippingDiscountAmount without checking for custom discounts)
                            form.ShippingTotal += discount.DiscountValue;
                            discount.Delete();
                        }
                    }
                }
            }

            foreach (OrderForm form in OrderGroup.OrderForms)
            {
                foreach (LineItem lineItem in form.LineItems)
                {
                    if (IsGiftLineItem(lineItem))
                    {
                        PurchaseOrderManager.RemoveLineItemFromOrder(OrderGroup, lineItem.LineItemId);
                    }
                    else
                    {
                        lineItem.OrderLevelDiscountAmount = GetCustomOrderDiscountAmount(form);
                        lineItem.LineItemDiscountAmount = GetCustomLineItemDiscountAmount(lineItem);
                        lineItem.ExtendedPrice = lineItem.ListPrice * lineItem.Quantity;
                    }
                }

                foreach (OrderFormDiscount discount in form.Discounts)
                {
                    if (!discount.DiscountName.StartsWith("@") /*&& discount.DiscountId == -1*/) // ignore custom entries
                    {
                        if (!string.IsNullOrEmpty(discount.DiscountCode))
                            coupons.Add(discount.DiscountCode);

                        discount.Delete();
                    }
                }
            }

            //add the coupons back to the promotion context
            if (coupons.Count > 0)
            {
                foreach (string coupon in coupons)
                    MarketingContext.Current.AddCouponToMarketingContext(coupon);
            }
        }

        private decimal GetCustomLineItemDiscountAmount(LineItem lineItem)
        {
            decimal retVal = 0m;
            foreach (LineItemDiscount lineItemDiscount in lineItem.Discounts)
            {
                if (lineItemDiscount.DiscountName.StartsWith("@"))
                {
                    retVal += lineItemDiscount.DiscountValue;
                }
            }
            return retVal;
        }

        private decimal GetCustomOrderDiscountAmount(OrderForm form)
        {
            decimal retVal = 0m;
            foreach (OrderFormDiscount orderFormDiscount in form.Discounts)
            {
                if (orderFormDiscount.DiscountName.StartsWith("@"))
                {
                    retVal += orderFormDiscount.DiscountValue;
                }
            }
            return retVal;
        }


        /// <summary>
        /// Determines whether [is gift line item] [the specified line item].
        /// </summary>
        /// <param name="lineItem">The line item.</param>
        /// <returns>
        ///     <c>true</c> if [is gift line item] [the specified line item]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsGiftLineItem(LineItem lineItem)
        {
            bool retVal = false;
            foreach (LineItemDiscount discount in lineItem.Discounts)
            {
                if (discount.DiscountName.EndsWith(":Gift"))
                {
                    retVal = true;
                    break;
                }
            }
            return retVal;
        }
    }
}