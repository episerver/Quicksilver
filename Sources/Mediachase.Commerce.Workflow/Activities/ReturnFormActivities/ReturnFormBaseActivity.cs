using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.WorkflowCompatibility;

namespace Mediachase.Commerce.Workflow.ReturnForm
{
    public abstract class ReturnFormBaseActivity: Activity
    {
        [ActivityFlowContextProperty]
        public OrderForm ReturnOrderForm { get; set; }

        protected ReturnFormStatus ReturnFormStatus
        {
            get
            {
                return ReturnFormStatusManager.GetReturnFormStatus(ReturnOrderForm);
            }
        }

        protected void ChangeReturnFormStatus(ReturnFormStatus newStatus)
        {
            ReturnOrderForm.Status = newStatus.ToString();
        }
    }
}
