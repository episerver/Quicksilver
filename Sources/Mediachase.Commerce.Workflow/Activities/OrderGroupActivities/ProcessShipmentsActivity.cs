using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Shared;
using Mediachase.Commerce.WorkflowCompatibility;
using System;
using System.Linq;

namespace Mediachase.Commerce.Workflow.Customization
{
    /// <summary>
    /// This activity is responsible for calculating the shipping prices for shipments defined for order group.
    /// It calls the appropriate interface defined by the shipping option table and passes the method id and shipment object.
    /// </summary>
    public class ProcessShipmentsActivity : OrderGroupActivityBase
    {
        [NonSerialized]
        private readonly ILogger Logger;

        #region Public Events

        /// <summary>
        /// Occurs when [processing shipment].
        /// </summary>
        public static string ProcessingShipmentEvent = "ProcessingShipment";

        /// <summary>
        /// Occurs when [processed shipment].
        /// </summary>
        public static string ProcessedShipmentEvent = "ProcessedShipment";
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessShipmentsActivity"/> class.
        /// </summary>
        public ProcessShipmentsActivity()
        {
            Logger = LogManager.GetLogger(GetType());
        }

        /// <summary>
        /// Called by the workflow runtime to execute an activity.
        /// </summary>
        /// <param name="executionContext">The <see cref="T:Mediachase.Commerce.WorkflowCompatibility.ActivityExecutionContext"/> to associate with this <see cref="T:Mediachase.Commerce.WorkflowCompatibility.Activity"/> and execution.</param>
        /// <returns>
        /// The <see cref="T:Mediachase.Commerce.WorkflowCompatibility.ActivityExecutionStatus"/> of the run task, which determines whether the activity remains in the executing state, or transitions to the closed state.
        /// </returns>
        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            // Raise the ProcessingShipmentEvent event to the parent workflow
            base.RaiseEvent(ProcessingShipmentEvent, EventArgs.Empty);

            // Validate the properties at runtime
            ValidateRuntime();

            // Process Shipment now
            ProcessShipments();

            // Raise the ProcessedShipmentEvent event to the parent workflow
            base.RaiseEvent(ProcessedShipmentEvent, EventArgs.Empty);

            // Retun the closed status indicating that this activity is complete.
            return ActivityExecutionStatus.Closed;
        }

        /// <summary>
        /// Processes the shipments.
        /// </summary>
        private void ProcessShipments()
        {
            ShippingMethodDto methods = ShippingManager.GetShippingMethods(/*Thread.CurrentThread.CurrentUICulture.Name*/String.Empty);

            OrderGroup order = OrderGroup;
            var billingCurrency = order.BillingCurrency;

            // request rates, make sure we request rates not bound to selected delivery method
            foreach (OrderForm form in order.OrderForms)
            {
                foreach (Shipment shipment in form.Shipments)
                {
                    bool processThisShipment = true;

                    string discountName = "@ShipmentSkipRateCalc";
                    // If you find the shipment discount which represents 
                    if (shipment.Discounts.Cast<ShipmentDiscount>().Any(x => x.ShipmentId == shipment.ShipmentId && x.DiscountName.Equals(discountName)))
                    {
                        processThisShipment = false;
                    }

                    if (!processThisShipment)
                    {
                        continue;
                    }

                    ShippingMethodDto.ShippingMethodRow row = methods.ShippingMethod.FindByShippingMethodId(shipment.ShippingMethodId);

                    // If shipping method is not found, set it to 0 and continue
                    if (row == null)
                    {
                        Logger.Information(String.Format("Total shipment is 0 so skip shipment calculations."));
                        shipment.ShippingSubTotal = 0;
                        continue;
                    }

                    // Check if package contains shippable items, if it does not use the default shipping method instead of the one specified
                    Logger.Debug(String.Format("Getting the type \"{0}\".", row.ShippingOptionRow.ClassName));
                    Type type = Type.GetType(row.ShippingOptionRow.ClassName);
                    if (type == null)
                    {
                        throw new TypeInitializationException(row.ShippingOptionRow.ClassName, null);
                    }
                    Logger.Debug(String.Format("Creating instance of \"{0}\".", type.Name));
                    IShippingGateway provider = null;
                    var orderMarket = ServiceLocator.Current.GetInstance<IMarketService>().GetMarket(order.MarketId);
                    if (orderMarket != null)
                    {
                        provider = (IShippingGateway)Activator.CreateInstance(type, orderMarket);
                    }
                    else
                    {
                        provider = (IShippingGateway)Activator.CreateInstance(type);
                    }

                    Logger.Debug(String.Format("Calculating the rates."));
                    string message = String.Empty;
                    ShippingRate rate = provider.GetRate(row.ShippingMethodId, shipment, ref message);
                    if (rate != null)
                    {
                        Logger.Debug(String.Format("Rates calculated."));
                        // check if shipment currency is convertable to Billing currency, and then convert it
                        if (!CurrencyFormatter.CanBeConverted(rate.Money, billingCurrency))
                        {
                            Logger.Debug(String.Format("Cannot convert selected shipping's currency({0}) to current currency({1}).", rate.Money.Currency.CurrencyCode, billingCurrency));
                            throw new Exception(String.Format("Cannot convert selected shipping's currency({0}) to current currency({1}).", rate.Money.Currency.CurrencyCode, billingCurrency));
                        }
                        else
                        {
                            Money convertedRate = CurrencyFormatter.ConvertCurrency(rate.Money, billingCurrency);
                            shipment.ShippingSubTotal = convertedRate.Amount;
                        }
                    }
                    else
                    {
                        Warnings[String.Concat("NoShipmentRateFound-", shipment.ShippingMethodName)] =
                            String.Concat("No rates have been found for ", shipment.ShippingMethodName);
                        Logger.Debug(String.Format("No rates have been found."));
                    }
                }
            }
        }
    }
}