using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Workflow.Activities;
using Mediachase.Commerce.Workflow.Activities.Cart;
using Mediachase.Commerce.Workflow.Activities.OrderGroupActivities;
using Mediachase.Commerce.Workflow.Activities.PurchaseOrderActivities;

namespace Mediachase.Commerce.Workflow
{
    /// <summary>
    /// Activity flow that should be used before the checkout phase. 
    /// </summary>
    [ActivityFlowConfiguration(Name = OrderGroupWorkflowManager.CartPrepareWorkflowName, AvailableInBetaMode = true)]
    public class CartPrepareVNextActivityFlow : ActivityFlow
    {
        /// <inheritdoc />
        public override ActivityFlowRunner Configure(ActivityFlowRunner activityFlow)
        {
            return
                activityFlow
                    .Do<ValidateLineItemsActivity>()
                    .Do<GetFulfillmentWarehouseActivity>()
                    .If(ShouldCheckInstoreInventory)
                        .Do<CheckInstoreInventoryActivity>()
                    .Else()
                        .Do<CheckInventoryActivity>()
                    .EndIf()
                    .Do<RemoveDiscountsVNextActivity>()
                    .Do<CalculateDiscountsVNextActivity>()
                    .Do<UpdateTotalsVNextActivity>();
        }
    }
}
