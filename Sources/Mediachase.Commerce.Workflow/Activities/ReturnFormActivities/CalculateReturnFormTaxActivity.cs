using Mediachase.Commerce.WorkflowCompatibility;

namespace Mediachase.Commerce.Workflow.Customization.ReturnForm
{
    public class CalculateReturnFormTaxActivity : ReturnFormBaseActivity
    {
        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            // Calculate sale tax
            CalculateSaleTaxes();

            // Retun the closed status indicating that this activity is complete.
            return ActivityExecutionStatus.Closed;
        }

        /// <summary>
        /// Calculates the sale taxes.
        /// </summary>
        private void CalculateSaleTaxes()
        {
            var order = base.ReturnOrderForm.Parent;
            OrderFormHelper.CalculateTaxes(base.ReturnOrderForm, order.MarketId, order.BillingCurrency);
        }
    }
}