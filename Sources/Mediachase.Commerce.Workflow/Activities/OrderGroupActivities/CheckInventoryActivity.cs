using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.WorkflowCompatibility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mediachase.Commerce.Workflow
{
    public class CheckInventoryActivity : OrderGroupActivityBase
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

            ValidateItems();

            // Retun the closed status indicating that this activity is complete.
            return ActivityExecutionStatus.Closed;
        }

        /// <summary>
        /// Validate inventory in the order group.
        /// </summary>
        /// <remarks>We don't need to validate quantity in the wishlist.</remarks>
        private void ValidateItems()
        {
            if (string.Equals(OrderGroup.Name, Mediachase.Commerce.Orders.Cart.WishListName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            foreach (OrderForm form in OrderGroup.OrderForms)
            {
                var deletedLineItems = new List<LineItem>();
                foreach (var shipment in form.Shipments.OfType<Shipment>())
                {
                    foreach (var lineItem in Shipment.GetShipmentLineItems(shipment))
                    {
                        if (lineItem.Code == "0" || string.IsNullOrEmpty(lineItem.Code) || lineItem.Code.StartsWith("@") || lineItem.IsGift)
                        {
                            continue;
                        }

                        AdjustInventory(lineItem, shipment, deletedLineItems);
                    }
                }

                //legacy line items without shipments
                for (var i = 0; i < form.LineItems.Count; i++)
                {
                    if (!form.Shipments.Any(x => x.LineItemIndexes.Contains(i.ToString())))
                    {
                        AdjustInventory(form.LineItems[i], null, deletedLineItems);
                    }
                }

                foreach (var lineItem in deletedLineItems)
                {
                    var index = form.LineItems.IndexOf(lineItem);
                    foreach (Shipment shipment in form.Shipments)
                    {
                        if (shipment.LineItemIndexes.Contains(index.ToString()))
                        {
                            shipment.RemoveLineItemIndex(index);
                        }
                    }
                    ReorderIndexes(form, lineItem);
                }

                foreach (var lineItem in deletedLineItems)
                {
                    lineItem.Delete();
                }
            }
        }

        private void AdjustInventory(LineItem lineItem, Shipment shipment, List<LineItem> deletedLineItems)
        {
            var changeQtyReason = new List<string>();

            decimal newQty;
            if (lineItem.IsInventoryAllocated)
            {
                if (shipment == null)
                {
                    newQty = lineItem.Quantity;
                }
                else
                {
                    newQty = Shipment.GetLineItemQuantity(shipment, lineItem.LineItemId);
                }
            }
            else
            {
                newQty = GetNewLineItemQty(lineItem, changeQtyReason, shipment);
            }

            var changeQtyReasonDisplay = String.Join(" and ", changeQtyReason.ToArray());
            if (newQty == 0)
            {
                // Remove item if it reached this stage
                Warnings.Add("LineItemRemoved-" + lineItem.LineItemId + (shipment == null ? "" : shipment.ShipmentId.ToString()),
                    string.Format(
                        "Item \"{0}\" has been removed from the cart because it is no longer available or there is not enough available quantity.",
                        lineItem.DisplayName));
                deletedLineItems.Add(lineItem);
            }
            else
            {
                decimal delta;
                if (shipment == null)
                {
                    delta = lineItem.Quantity - newQty;
                }
                else
                {
                    delta = Shipment.GetLineItemQuantity(shipment, lineItem.LineItemId) - newQty;
                }

                if (delta == 0)
                { 
                    return;
                }

                lineItem.Quantity -= delta;
                ChangeShipmentsLineItemQuantity(lineItem, delta, shipment);
                Warnings.Add("LineItemQtyChanged-" + lineItem.LineItemId + (shipment == null ? "" : shipment.ShipmentId.ToString()),
                    string.Format("Item \"{0}\" quantity has been changed {1}", lineItem.DisplayName, changeQtyReasonDisplay));
            }
        }

        private void ChangeShipmentsLineItemQuantity(LineItem lineItem, decimal delta, Shipment shipment)
        {
            if (shipment == null)
            {
                return;
            }
            //Decrease qty in all shipment contains line item
            var shipmentQty = Shipment.GetLineItemQuantity(shipment, lineItem.LineItemId);
            var newShipmentQty = shipmentQty - delta;
            newShipmentQty = newShipmentQty > 0 ? newShipmentQty : 0;
            shipment.SetLineItemQuantity(shipment.Parent.LineItems.IndexOf(lineItem), newShipmentQty);
        }
    }
}