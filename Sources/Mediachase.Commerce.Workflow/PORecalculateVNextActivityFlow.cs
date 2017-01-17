using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Workflow.Activities;
using Mediachase.Commerce.Workflow.Activities.Cart;
using Mediachase.Commerce.Workflow.Activities.OrderGroupActivities;
using Mediachase.Commerce.Workflow.Activities.PurchaseOrderActivities;

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
