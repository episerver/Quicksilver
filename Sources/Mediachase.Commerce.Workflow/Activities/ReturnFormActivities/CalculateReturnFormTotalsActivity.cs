using Mediachase.Commerce.Orders;
using Mediachase.Commerce.WorkflowCompatibility;

namespace Mediachase.Commerce.Workflow.Customization.ReturnForm
{
    public class CalculateReturnFormTotalsActivity : ReturnFormBaseActivity
    {
        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            // Calculate order totals
            CalculateTotalsReturnOrderForm();

            // Retun the closed status indicating that this activity is complete.
            return ActivityExecutionStatus.Closed;
        }

        /// <summary>
        /// Calculates the totals order forms.
        /// </summary>
        private void CalculateTotalsReturnOrderForm()
        {
            var currency = new Currency(ReturnOrderForm.Parent.BillingCurrency);
            decimal subTotal = 0m;
            decimal discountTotal = 0m;
            decimal shippingDiscountTotal = 0m;
            decimal shippingTotal = 0m;

            foreach (LineItem item in base.ReturnOrderForm.LineItems)
            {
                item.ExtendedPrice = item.PlacedPrice * item.ReturnQuantity - currency.Round((item.LineItemDiscountAmount + item.OrderLevelDiscountAmount) / item.Quantity * item.ReturnQuantity);
                subTotal += item.PlacedPrice * item.ReturnQuantity - currency.Round(item.LineItemDiscountAmount / item.Quantity * item.ReturnQuantity);
                discountTotal += currency.Round(item.OrderLevelDiscountAmount / item.Quantity * item.ReturnQuantity);
            }

            foreach (Shipment shipment in base.ReturnOrderForm.Shipments)
            {
                shippingTotal += shipment.ShippingSubTotal;
                shippingDiscountTotal += shipment.ShippingDiscountAmount;
            }

            discountTotal += shippingDiscountTotal;

            ReturnOrderForm.ShippingTotal = shippingTotal;
            ReturnOrderForm.DiscountAmount = discountTotal;
            ReturnOrderForm.SubTotal = subTotal;

            ReturnOrderForm.Total = subTotal + shippingTotal + ReturnOrderForm.TaxTotal - discountTotal;
        }

    }
}