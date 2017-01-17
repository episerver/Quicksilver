using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Workflow.Activities;
using Mediachase.Commerce.Workflow.Activities.Cart;

namespace Mediachase.Commerce.Workflow
{
    /// <summary>
    /// This class represents the Purchase Order Calculate Totals workflow
    /// </summary>
    [ActivityFlowConfiguration(Name = OrderGroupWorkflowManager.OrderCalculateTotalsWorkflowName)]
    public class POCalculateTotalsActivityFlow : ActivityFlow
    {
        /// <inheritdoc />
        public override ActivityFlowRunner Configure(ActivityFlowRunner activityFlow)
        {
            return
                activityFlow
                    .Do<ProcessShipmentsActivity>()
                    .Do<CalculateTotalsActivity>()
                    .Do<RemoveDiscountsActivity>()
                    .Do<CalculateTotalsActivity>()
                    .Do<CalculateDiscountsActivity>()
                    .Do<CalculateTotalsActivity>()
                    .Do<CalculateTaxActivity>()
                    .Do<CalculateTotalsActivity>()
                    .Do<RecordPromotionUsageActivity>();
        }
    }
}
