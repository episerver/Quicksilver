using EPiServer.Commerce.Order.Internal;
using Mediachase.Commerce.WorkflowCompatibility;
using System.Linq;
using EPiServer.Commerce.Order;

namespace Mediachase.Commerce.Workflow.Customization
{
    /// <summary>
    /// Activity for removing discounts.
    /// </summary>
    public class RemoveDiscountsVNextActivity : OrderGroupActivityBase
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
            ValidateRuntime();
            RemoveDiscounts();
            return ActivityExecutionStatus.Closed;
        }

        /// <summary>
        /// Removes the discounts.
        /// </summary>
        private void RemoveDiscounts()
        {
            var order = OrderGroup as IOrderGroup;
            if (order == null)
            {
                return;
            }

            foreach (var shipment in order.Forms.SelectMany(x => x.Shipments))
            {
                shipment.TrySetDiscountValue(x => x.ShipmentDiscount, 0);
                foreach (var lineItem in shipment.LineItems)
                {
                    lineItem.TrySetDiscountValue(x => x.EntryAmount, 0);
                    lineItem.TrySetDiscountValue(x => x.OrderAmount, 0);
                }
            }
            
            var orderFormLineItems = OrderGroup.OrderForms.SelectMany(x => x.LineItems);

            foreach (var lineItem in orderFormLineItems)
            {
                lineItem.TrySetDiscountValue(x => x.EntryAmount, 0);
                lineItem.TrySetDiscountValue(x => x.OrderAmount, 0);
            }

            foreach (var promotion in order.Forms.SelectMany(f => f.Promotions))
            {
                promotion.ClearSavedAmount();
            }
        }
    }
}
