using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Workflow;
using Mediachase.Commerce.Workflow.OrderGroupActivities;

namespace Mediachase.Commerce.Workflow
{
    /// <summary>
    /// Activity flow that is used to update all the discounts for an order.
    /// </summary>
    [ActivityFlowConfiguration(Name = OrderGroupWorkflowManager.OrderCalculateTotalsWorkflowName, AvailableInBetaMode = true)]
    public class POCalculateTotalsVNextActivityFlow : ActivityFlow
    {
        /// <inheritdoc />
        public override ActivityFlowRunner Configure(ActivityFlowRunner activityFlow)
        {
            return
                activityFlow
                    .Do<RemoveDiscountsVNextActivity>()
                    .Do<CalculateDiscountsVNextActivity>()
                    .Do<UpdateTotalsVNextActivity>();
        }
    }
}
