using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Workflow;
using Mediachase.Commerce.Workflow.Cart;

namespace Mediachase.Commerce.Workflow
{
    /// <summary>
    /// This class represents the Purchase Order Save Changes workflow
    /// </summary>
    [ActivityFlowConfiguration(Name = OrderGroupWorkflowManager.OrderSaveChangesWorkflowName)]
    public class POSaveChangesActivityFlow : ActivityFlow
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
                    .Do<CalculateTotalsActivity>();
        }
    }
}
