using EPiServer.ServiceLocation;
using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Workflow.Activities.PurchaseOrderActivities;
using Mediachase.Commerce.WorkflowCompatibility;
using System.Collections.Specialized;

namespace Mediachase.Commerce.Workflow
{
    /// <summary>
    /// This class represents the CheckAndReserveInstorePickup workflow.
    /// It checks Inventory for in-store pickup and reserves items accordingly
    /// </summary>
    [ActivityFlowConfiguration(Name = OrderGroupWorkflowManager.CheckAndReserveInstorePickupWorkflowName)]
    public class CheckAndReserveInstorePickupActivityFlow : ActivityFlow
    {
        /// <inheritdoc />
        public override ActivityFlowRunner Configure(ActivityFlowRunner activityFlow)
        {
            return activityFlow.Do<CheckInstoreInventoryActivity>();
        }
    }
}
