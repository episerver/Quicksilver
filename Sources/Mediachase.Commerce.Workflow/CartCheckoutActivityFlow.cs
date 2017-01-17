using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Workflow.Activities;
using Mediachase.Commerce.Workflow.Activities.Cart;

namespace Mediachase.Commerce.Workflow
{
    /// <summary>
    /// This class represents the Cart Checkout workflow
    /// </summary>
    [ActivityFlowConfiguration(Name = OrderGroupWorkflowManager.CartCheckOutWorkflowName)]
    public class CartCheckoutActivityFlow : ActivityFlow
    {
        /// <inheritdoc />
        public override ActivityFlowRunner Configure(ActivityFlowRunner activityFlow)
        {
            return
                activityFlow
                    .If(ShouldProcessPayment)
                        .Do<ProcessPaymentActivity>()
                    .EndIf()
                    .Do<CalculateTotalsActivity>()
                    .Do<AdjustInventoryActivity>()
                    .Do<RecordPromotionUsageActivity>();
        }
    }
}
