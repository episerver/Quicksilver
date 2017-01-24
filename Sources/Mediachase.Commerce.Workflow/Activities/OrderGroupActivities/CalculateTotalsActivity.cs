using EPiServer.Commerce.Order;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.WorkflowCompatibility;
using Mediachase.MetaDataPlus;
using System.Linq;

namespace Mediachase.Commerce.Workflow
{
    public class CalculateTotalsActivity : OrderGroupActivityBase
    {
        private Currency _billingCurrency;

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

            // Calculate order totals
            CalculateTotals();

            // Retun the closed status indicating that this activity is complete.
            return ActivityExecutionStatus.Closed;
        }

        /// <summary>
        /// Calculates the totals.
        /// </summary>
        private void CalculateTotals()
        {
            if (OrderGroup.ObjectState == MetaObjectState.Deleted)
            {
                return;
            }

            decimal subTotal = 0m;
            decimal shippingTotal = 0m;
            decimal handlingTotal = 0m;
            decimal taxTotal = 0m;
            decimal total = 0m;

            // Get the property, since it is expensive process, make sure to get it once
            OrderGroup order = OrderGroup;
            _billingCurrency = order.BillingCurrency;

            // Calculate totals for OrderForms
            foreach (OrderForm form in order.OrderForms)
            {
                // Calculate totals for order form
                CalculateTotalsOrderForms(form);

                subTotal += form.SubTotal;
                shippingTotal += form.ShippingTotal;
                handlingTotal += form.HandlingTotal;
                taxTotal += form.TaxTotal;
                total += form.Total;
            }

            // calculate OrderGroup totals
            order.SubTotal = subTotal;
            order.ShippingTotal = shippingTotal;
            order.TaxTotal = taxTotal;
            order.Total = total;
            order.HandlingTotal = handlingTotal;
        }

        /// <summary>
        /// Calculates the totals order forms.
        /// </summary>
        /// <param name="form">The form.</param>
        private void CalculateTotalsOrderForms(OrderForm form)
        {
            decimal subTotal = 0m;
            decimal discountTotal = 0m;
            decimal shippingDiscountTotal = 0m;
            decimal shippingTotal = 0m;

            foreach (LineItem item in form.LineItems.Where(x => x.ObjectState != MetaObjectState.Deleted))
            {
                decimal lineItemDiscount = item.LineItemDiscountAmount + item.OrderLevelDiscountAmount;
                item.ExtendedPrice = item.PlacedPrice * item.Quantity - lineItemDiscount;
                subTotal += item.ExtendedPrice;
                discountTotal += lineItemDiscount;
            }

            foreach (Shipment shipment in form.Shipments.Where(x => x.ObjectState != MetaObjectState.Deleted))
            {
                shipment.SubTotal = CalculateShipmentSubtotal(shipment);
                shippingTotal += shipment.ShippingSubTotal;
                shippingTotal -= shipment.ShippingDiscountAmount;
                shippingDiscountTotal += shipment.ShippingDiscountAmount;
            }

            form.ShippingTotal = shippingTotal;
            form.DiscountAmount = discountTotal + shippingDiscountTotal;
            form.SubTotal = subTotal;

            form.Total = subTotal + shippingTotal + form.TaxTotal;

            form.UpdatePaymentTotals();
        }

        private decimal CalculateShipmentSubtotal(Shipment shipment)
        {
            var retVal = 0m;
            var lineItems = Shipment.GetShipmentLineItems(shipment).Where(item => item.Quantity > 0);
            retVal = lineItems.Sum(item => _billingCurrency.Round(item.ExtendedPrice / item.Quantity * Shipment.GetLineItemQuantity(shipment, item.LineItemId)));
            return retVal;
        }
    }
}