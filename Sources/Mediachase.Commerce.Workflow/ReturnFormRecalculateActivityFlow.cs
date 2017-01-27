using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Workflow.ReturnForm;

namespace Mediachase.Commerce.Workflow
{
    /// <summary>
    /// This class represents the ReturnFormRecalculate workflow
    /// </summary>
    [ActivityFlowConfiguration(Name = OrderGroupWorkflowManager.ReturnFormRecalculateWorkflowName)]
    public class ReturnFormRecalculateActivityFlow : ActivityFlow
    {
        /// <inheritdoc />
        public override ActivityFlowRunner Configure(ActivityFlowRunner activityFlow)
        {
            return
                activityFlow
                    .Do<CalculateReturnFormTotalsActivity>()
                    .Do<CalculateReturnFormTaxActivity>()
                    .Do<CalculateReturnFormTotalsActivity>()
                    .Do<CalculateReturnFormStatusActivity>()
                    .Do<CalculateExchangeOrderStatusActivity>();
        }
    }
}
