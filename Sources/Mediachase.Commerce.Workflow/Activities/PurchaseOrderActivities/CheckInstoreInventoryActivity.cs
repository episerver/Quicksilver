using Mediachase.Commerce.Orders;
using Mediachase.Commerce.WorkflowCompatibility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mediachase.Commerce.Workflow.PurchaseOrderActivities
{
    public class CheckInstoreInventoryActivity : HandoffActivityBase
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

            var orderForms = OrderGroup.OrderForms.ToArray();
            IEnumerable<Shipment> shipments = orderForms.SelectMany(of => of.Shipments.ToArray());

            IEnumerable<LineItem> lineItems = orderForms.SelectMany(x => x.LineItems.ToArray());

            IEnumerable<LineItem> validLineItems = lineItems.Where(x => x.Code != "0" && !String.IsNullOrEmpty(x.Code) && !x.Code.StartsWith("@") && !x.IsGift);

            List<LineItem> lineItemsToDelete = new List<LineItem>();

            foreach (Shipment shipment in shipments)
            {
                foreach (LineItem lineItem in validLineItems)
                {
                    // check if line item belongs to current shipment
                    var shipmentQty = Shipment.GetLineItemQuantity(shipment, lineItem.LineItemId);
                    if (shipmentQty == 0)
                    {
                        continue;
                    }

                    List<string> changeQtyReason = new List<string>();

                    decimal newQty;
                    if (lineItem.IsInventoryAllocated)
                    {
                        newQty = shipmentQty;
                    }
                    else
                    {
                        newQty = GetNewLineItemQty(lineItem, changeQtyReason, shipment);
                    }

                    var changeQtyReasonDisplay = String.Join(" and ", changeQtyReason.ToArray());
                    if (newQty == 0)
                    {
                        // Remove item if it reached this stage
                        Warnings.Add("Shipment-" + shipment.Id.ToString() + "-LineItemRemoved-" + lineItem.Id.ToString(),
                            String.Format("Item \"{0}\" has been removed from the cart because it is no longer available.", lineItem.DisplayName));
                        DeleteLineItemFromShipment(shipment, lineItem);
                        // Delete line item, but only if it belongs to no shipments
                        if (!orderForms.SelectMany(of => of.Shipments.ToArray()).Where(s => Shipment.GetLineItemQuantity(s, lineItem.LineItemId) > 0).Any())
                        {
                            lineItemsToDelete.Add(lineItem);
                        }
                    }
                    else
                    {
                        var delta = shipmentQty - newQty;
                        if (delta != 0)
                        {
                            lineItem.Quantity -= delta;
                            ChangeShipmentLineItemQty(shipment, lineItem, delta);
                            Warnings.Add("Shipment-" + shipment.Id.ToString() + "-LineItemQtyChanged-" + lineItem.Id.ToString(),
                                         String.Format("Item \"{0}\" quantity has been changed {1}.", lineItem.DisplayName, changeQtyReasonDisplay));
                        }
                    }
                }
            }

            foreach (LineItem lineItem in lineItemsToDelete)
            {
                lineItem.Delete();
            }
        }

        private void DeleteLineItemFromShipment(Shipment shipment, LineItem lineItem)
        {
            var orderForm = OrderGroup.OrderForms.ToArray().Where(of => of.LineItems.ToArray().Contains(lineItem)).FirstOrDefault();
            if (orderForm != null)
            {
                var lineItemIndex = orderForm.LineItems.IndexOf(lineItem);
                decimal shipmentQty = Shipment.GetLineItemQuantity(shipment, lineItem.LineItemId);
                lineItem.Quantity -= shipmentQty;
                shipment.RemoveLineItemIndex(lineItemIndex);

                ReorderIndexes(orderForm, lineItem);
            }
        }

        private void ChangeShipmentLineItemQty(Shipment shipment, LineItem lineItem, decimal delta)
        {
            var orderForm = OrderGroup.OrderForms.ToArray().Where(of => of.LineItems.ToArray().Contains(lineItem)).FirstOrDefault();
            if (orderForm != null)
            {
                var lineItemIndex = orderForm.LineItems.IndexOf(lineItem);
                {
                    //Decrease qty in all shipment contains line item
                    var shipmentQty = Shipment.GetLineItemQuantity(shipment, lineItem.LineItemId);
                    var newShipmentQty = shipmentQty - delta;
                    newShipmentQty = newShipmentQty > 0 ? newShipmentQty : 0;
                    //Set new line item qty in shipment
                    shipment.SetLineItemQuantity(lineItemIndex, newShipmentQty);
                    delta -= Math.Min(delta, shipmentQty);
                }
            }
        }
    }
}