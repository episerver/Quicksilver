using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Workflow;
using Mediachase.Commerce.Workflow.Cart;
using Mediachase.Commerce.Workflow.OrderGroupActivities;
using Mediachase.Commerce.Workflow.PurchaseOrderActivities;

namespace Mediachase.Commerce.Workflow
{
    /// <summary>
    /// Activity flow that validates and updates a purchase order.
    /// </summary>
    [ActivityFlowConfiguration(Name = OrderGroupWorkflowManager.OrderRecalculateWorkflowName, AvailableInBetaMode = true)]
    public class PORecalculateVNextActivityFlow : ActivityFlow
    {
        public override ActivityFlowRunner Configure(ActivityFlowRunner activityFlow)
        {
            return
                activityFlow
                    .Do<ValidateLineItemsActivity>()
                    .Do<GetFulfillmentWarehouseActivity>()
                    .If(ShouldAdjustInventory)
                        .If(ShouldCheckInstoreInventory)
                            .Do<CheckInstoreInventoryActivity>()
                        .Else()
                            .Do<CheckInventoryActivity>()
                        .EndIf()
                    .EndIf()
                    .If(ShouldAdjustInventory)
                        .Do<AdjustInventoryActivity>()
                    .EndIf()
                    .If(ShouldRecalculateOrder)
                        .Do<RemoveDiscountsVNextActivity>()
                        .Do<CalculateDiscountsVNextActivity>()
                        .Do<UpdateTotalsVNextActivity>()
                    .EndIf()
                    .Do<CalculatePurchaseOrderStatusActivity>();
        }
    }
}
