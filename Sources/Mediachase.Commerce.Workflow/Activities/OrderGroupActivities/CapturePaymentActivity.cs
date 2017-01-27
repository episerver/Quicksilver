using EPiServer.Commerce.Order;
using EPiServer.Logging;
using Mediachase.BusinessFoundation.Data;
using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.WorkflowCompatibility;
using Mediachase.MetaDataPlus.Configurator;
using System;
using System.Linq;

namespace Mediachase.Commerce.Workflow
{
    /// <summary>
    /// This activity is responsible for calculating the shipping prices for Payments defined for order group.
    /// It calls the appropriate interface defined by the shipping option table and passes the method id and Payment object.
    /// </summary>
    public class CapturePaymentActivity : OrderGroupActivityBase
    {
        private const string EventsCategory = "Handlers";

        [NonSerialized]
        private readonly ILogger _logger;

        private Currency _billingCurrency;

        /// <summary>
        /// Gets or sets the shipment.
        /// </summary>
        /// <value>The shipment.</value>
        [ActivityFlowContextProperty]
        public Shipment Shipment { get; set; }

        #region Public Events

        /// <summary>
        /// Occurs when [processing payment].
        /// </summary>
        public static string ProcessingPaymentEvent = "ProcessingPayment";

        /// <summary>
        /// Occurs when [processed payment].
        /// </summary>
        public static string ProcessedPaymentEvent = "ProcessedPayment";

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="CapturePaymentActivity"/> class.
        /// </summary>
        public CapturePaymentActivity()
        {
            _logger = LogManager.GetLogger(GetType());
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
            // Raise the ProcessingPaymentEvent event to the parent workflow
            RaiseEvent(ProcessingPaymentEvent, EventArgs.Empty);

            _billingCurrency = OrderGroup.BillingCurrency;

            var orderForm = OrderGroup.OrderForms[0];
            if (orderForm.CapturedPaymentTotal < orderForm.Total)
            {
                // Validate the properties at runtime
                ValidateRuntime();

                // Process payment now
                ProcessPayment();
            }

            // Raise the ProcessedPaymentEvent event to the parent workflow
            RaiseEvent(ProcessedPaymentEvent, EventArgs.Empty);

            // Retun the closed status indicating that this activity is complete.
            return ActivityExecutionStatus.Closed;
        }

        /// <summary>
        /// Validates the order properties.
        /// </summary>
        /// <param name="validationErrors">The validation errors.</param>
        protected override void ValidateOrderProperties(ValidationErrorCollection validationErrors)
        {
            // Validate the To property
            if (OrderGroup == null)
            {
                validationErrors.Add(ValidationError.GetNotSetValidationError("OrderGroup"));
            }

            var orderForm = OrderGroup.OrderForms[0];
            var totalPaid = orderForm.AuthorizedPaymentTotal + orderForm.CapturedPaymentTotal;
            var shipmentTotal = CalculateShipmentTotal();
            if (totalPaid < shipmentTotal)
            {
                var errorText = string.Format("Incorrect authorization total. Expected: {0}, but Actual is: {1}", totalPaid, shipmentTotal);

                _logger.Error(errorText);

                validationErrors.Add(new ValidationError(errorText, 205, false));
            }
        }

        /// <summary>
        /// Processes the payment.
        /// </summary>
        private void ProcessPayment()
        {
            // Calculate payment total
            var formPayments = OrderGroup.OrderForms[0].Payments.ToArray();
            var resultingAuthorizedPayments = OrderGroup.OrderForms[0].GetPaymentsByTransactionType(formPayments, TransactionType.Authorization);
            var authorizedPayments = resultingAuthorizedPayments.Where(x => PaymentStatusManager.GetPaymentStatus(x) == PaymentStatus.Processed);

            // Find intire authorization
            var shipmentTotal = CalculateShipmentTotal();
            var intirePayment = authorizedPayments.OrderBy(x => x.Amount).FirstOrDefault(x => x.Amount >= shipmentTotal);
            if (intirePayment == null)
            {
                var payments = authorizedPayments.OrderByDescending(x => x.Amount);
                foreach (Payment partialPayment in payments)
                {
                    if (partialPayment.Amount < shipmentTotal)
                    {
                        DoCapture(partialPayment, partialPayment.Amount);
                        shipmentTotal -= partialPayment.Amount;
                    }
                    else
                    {
                        DoCapture(partialPayment, shipmentTotal);
                        break;
                    }
                }
            }
            else
            {
                DoCapture((Payment)intirePayment, shipmentTotal);
            }
        }

        private decimal CalculateShipmentTotal()
        {
            return Shipment.SubTotal + Shipment.ShippingTotal + OrderFormHelper.CalculateSalesTaxTotal(OrderGroup.MarketId, Shipment, _billingCurrency).Amount;
        }

        private void DoCapture(Payment authorizePayment, decimal amount)
        {
            if (string.IsNullOrEmpty(authorizePayment.TransactionID))
            {
                authorizePayment.TransactionID = Guid.NewGuid().ToString();
            }

            var paymentMethodDto = PaymentManager.GetPaymentMethod(authorizePayment.PaymentMethodId, true);
            var paymentType = AssemblyUtil.LoadType(paymentMethodDto.PaymentMethod[0].PaymentImplementationClassName);
            var payment = OrderGroup.OrderForms[0].Payments.AddNew(paymentType);
            foreach (MetaField field in authorizePayment.MetaClass.MetaFields)
            {
                if (!field.Name.Equals("PaymentId", StringComparison.InvariantCultureIgnoreCase))
                {
                    payment[field.Name] = authorizePayment[field.Name];
                }
            }

            payment.Amount = amount;
            payment.TransactionType = TransactionType.Capture.ToString();
            payment.Status = PaymentStatus.Pending.ToString();
        }
    }
}