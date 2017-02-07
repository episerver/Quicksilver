using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.WorkflowCompatibility;

namespace Mediachase.Commerce.Workflow.ReturnForm
{
    public class CalculateReturnFormTaxActivity : ReturnFormBaseActivity
    {
        private readonly Injected<ITaxCalculator> _taxCalculator;
        private readonly Injected<IMarketService> _marketService;

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
            Currency currency = order.BillingCurrency;
            var market = _marketService.Service.GetMarket(order.MarketId);

            var form = base.ReturnOrderForm;

            foreach (Shipment shipment in form.Shipments)
            {
                shipment.ShippingTax = currency.Round(_taxCalculator.Service.GetShippingTaxTotal(shipment, market, currency).Amount);
            }
            form.TaxTotal = currency.Round(_taxCalculator.Service.GetTaxTotal(form, market, currency).Amount);
        }
    }
}