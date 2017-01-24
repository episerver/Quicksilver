using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Workflow.Cart;
using Mediachase.Commerce.Workflow.OrderGroupActivities;

namespace Mediachase.Commerce.Workflow
{
    /// <summary>
    /// This class represents the Purchase Order Save Changes workflow
    /// </summary>
    [ActivityFlowConfiguration(Name = OrderGroupWorkflowManager.OrderSaveChangesWorkflowName, AvailableInBetaMode = true)]
    public class POSaveChangesVNextActivityFlow : ActivityFlow
    {
        /// <inheritdoc />
        public override ActivityFlowRunner Configure(ActivityFlowRunner activityFlow)
        {
            return
                activityFlow
                    .Do<AdjustInventoryActivity>()
                    .If(ShouldProcessPayment)
                        .Do<ProcessPaymentActivity>()
                    .EndIf()
                    .Do<UpdateTotalsVNextActivity>();
        }
    }
}
