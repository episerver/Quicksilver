using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Workflow;
using Mediachase.Commerce.Workflow.Cart;
using Mediachase.Commerce.Workflow.PurchaseOrderActivities;

namespace Mediachase.Commerce.Workflow
{
    /// <summary>
    /// This class represents the Cart Validate workflow
    /// </summary>
    [ActivityFlowConfiguration(Name = OrderGroupWorkflowManager.CartValidateWorkflowName)]
    public class CartValidateActivityFlow : ActivityFlow
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
                    .Do<CalculateTotalsActivity>()
                    .Do<RemoveDiscountsActivity>()
                    .Do<CalculateTotalsActivity>()
                    .Do<CalculateDiscountsActivity>()
                    .Do<CalculateTotalsActivity>()
                    .Do<RecordPromotionUsageActivity>();
        }
    }
}
