using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.WorkflowCompatibility;

namespace Mediachase.Commerce.Workflow
{
    public class CalculateTaxActivity : OrderGroupActivityBase
    {
        private readonly Injected<ITaxCalculator> _taxCalculator;
        private readonly Injected<IMarketService> _marketService;

        /// <summary>
        /// Called by the workflow runtime to execute an activity.
        /// </summary>
        /// <param name="executionContext">The <see cref="T:Mediachase.Commerce.WorkflowCompatibility.ActivityExecutionContext"/> to associate with this <see cref="T:Mediachase.Commerce.WorkflowCompatibility.Activity"/> and execution.</param>
        /// <returns>
        /// The <see cref="T:Mediachase.Commerce.WorkflowCompatibility.ActivityExecutionStatus"/> of the run task, which determines whether the activity remains in the executing state, or transitions to the closed state.
        /// </returns>
        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            // Validate the properties at runtime
            ValidateRuntime();

            // Calculate taxes
            CalculateTaxes();

            // Return the closed status indicating that this activity is complete.
            return ActivityExecutionStatus.Closed;
        }

        /// <summary>
        /// Calculates the sale and shipping taxes.
        /// </summary>
        private void CalculateTaxes()
        {
            // Get the property, since it is expensive process, make sure to get it once
            OrderGroup order = OrderGroup;
            Currency currency = order.BillingCurrency;
            var market = _marketService.Service.GetMarket(order.MarketId);

            foreach (OrderForm form in order.OrderForms)
            {
                foreach (Shipment shipment in form.Shipments)
                {
                    shipment.ShippingTax = currency.Round(_taxCalculator.Service.GetShippingTaxTotal(shipment, market, currency).Amount);
                }
                form.TaxTotal = currency.Round(_taxCalculator.Service.GetTaxTotal(form, market, currency).Amount);
            }
        }
    }
}