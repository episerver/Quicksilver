using Mediachase.Commerce.Orders;
using Mediachase.Commerce.WorkflowCompatibility;
using System;
using System.Linq;

namespace Mediachase.Commerce.Workflow.Customization.PurchaseOrderActivities
{
    public class CalculatePurchaseOrderStatusActivity : PurchaseOrderBaseActivity
    {
        /// <summary>
        /// Executes the activity.
        /// </summary>
        /// <param name="executionContext">The <see cref="T:Mediachase.Commerce.WorkflowCompatibility.ActivityExecutionContext"/> to associate with this <see cref="T:Mediachase.Commerce.WorkflowCompatibility.Activity"/> and execution.</param>
        /// <returns>
        /// The <see cref="T:Mediachase.Commerce.WorkflowCompatibility.ActivityExecutionStatus"/> of the run task, which determines whether the activity remains in the executing state, or transitions to the closed state.
        /// </returns>
        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (OrderGroup == null)
            {
                throw new NullReferenceException(OrderGroup is PurchaseOrder ? "purchaseOrder" : "PaymentPlan");
            }

            CalculateOrderShipmentsStatusesByOrderStatus(base.OrderStatus);

            //Calculate status only for specified statuses
            if ((base.OrderStatus & (OrderStatus.InProgress |
                                OrderStatus.PartiallyShipped)) == base.OrderStatus)
            {
                var newStatus = CalculateOrderGroupStatus();
                if (newStatus != base.OrderStatus)
                {
                    base.ChangeOrderStatus(newStatus);
                }
            }
            // Retun the closed status indicating that this activity is complete.
            return ActivityExecutionStatus.Closed;
        }

        private OrderStatus CalculateOrderGroupStatus()
        {
            OrderStatus retVal = base.OrderStatus;
            var shipments = base.OrderGroup.OrderForms[0].Shipments.ToArray();
            //var purchaseOrder = base.OrderGroup as Mediachase.Commerce.Orders.PurchaseOrder;

            OrderShipmentStatus avtiveStates = OrderShipmentStatus.InventoryAssigned | OrderShipmentStatus.AwaitingInventory
                                               | OrderShipmentStatus.Released | OrderShipmentStatus.Packing;

            bool canceledFound = shipments.Any(x => GetShipmentStatus(x) == OrderShipmentStatus.Cancelled);
            bool completedFound = shipments.Any(x => GetShipmentStatus(x) == OrderShipmentStatus.Shipped);
            bool activeFound = shipments.Any(x => (GetShipmentStatus(x) & avtiveStates) == GetShipmentStatus(x));

            if (!canceledFound && !completedFound && !activeFound)
            {
                //Not found canceled,active,completed

            }
            else if (canceledFound && !completedFound && !activeFound)
            {
                //All canceled
                //retVal = OrderStatus.Cancelled;
            }
            else if (!canceledFound && completedFound && !activeFound)
            {
                //All completed
                retVal = OrderStatus.Completed;
            }
            else if (!canceledFound && !completedFound && activeFound)
            {
                //All active
            }
            else if (canceledFound && completedFound && !activeFound)
            {
                //Found  cancelled and completed
                retVal = OrderStatus.Completed;
            }
            else if (canceledFound && !completedFound && activeFound)
            {
                //Found cancelled and active
            }
            else if (!canceledFound && completedFound && activeFound)
            {
                //Found completted and active
                retVal = OrderStatus.PartiallyShipped;
            }
            else if (canceledFound && completedFound && activeFound)
            {
                //Found cancelled and active and completed
                retVal = OrderStatus.PartiallyShipped;
            }

            return retVal;
        }

        private void CalculateOrderShipmentsStatusesByOrderStatus(OrderStatus orderStatus)
        {
            var shipments = base.OrderGroup.OrderForms[0].Shipments.ToArray();

            foreach (Shipment shipment in shipments)
            {
                if (string.IsNullOrEmpty(shipment.Status))
                {
                    shipment.Status = OrderShipmentStatus.InventoryAssigned.ToString();
                }
                //End states
                if (shipment.Status == OrderShipmentStatus.Shipped.ToString() ||
                    shipment.Status == OrderShipmentStatus.Cancelled.ToString())
                {
                    continue;
                }
                //Inherit order state for shipment
                if (orderStatus == OrderStatus.Cancelled)
                {
                    shipment.Status = OrderShipmentStatus.Cancelled.ToString();
                }
                else if (orderStatus == OrderStatus.OnHold)
                {
                    shipment.Status = OrderShipmentStatus.OnHold.ToString();
                }
                else
                {
                    //Restore back from onHold to prev state
                    if (shipment.Status == OrderShipmentStatus.OnHold.ToString())
                    {
                        shipment.Status = shipment.PrevStatus;
                    }
                }
            }
        }
    }
}