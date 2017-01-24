using Mediachase.Commerce.Orders;
using Mediachase.Commerce.WorkflowCompatibility;
using System;
using System.Linq;

namespace Mediachase.Commerce.Workflow.PurchaseOrderActivities
{
    [Obsolete("The AdjustInventoryActivity handled in store inventory adjustment now. Use the AdjustInventoryActivity instead. This activity will be removed in version 10.")]
    public partial class AdjustInstoreInventoryActivity : HandoffActivityBase
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
            // Check for multiple warehouses. In the default, we simply reject processing an order if the application has
            //  multiple warehouses. Any corresponding fulfillment process is the responsibility of the client.
            CheckMultiWarehouse();

            // Validate the properties at runtime
            ValidateRuntime();

            var orderForms = OrderGroup.OrderForms.Where(o => !OrderForm.IsReturnOrderForm(o));

            foreach (OrderForm orderForm in orderForms)
            {
                foreach (Shipment shipment in orderForm.Shipments)
                {
                    foreach (var lineItem in Shipment.GetShipmentLineItems(shipment))
                    {
                        AdjustStockItemQuantity(shipment, lineItem);
                    }
                }
            }

            // Retun the closed status indicating that this activity is complete.
            return ActivityExecutionStatus.Closed;
        }
    }
}