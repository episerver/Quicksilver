using EPiServer.Logging;
using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Exceptions;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.WorkflowCompatibility;
using System;
using System.Collections.Generic;

namespace Mediachase.Commerce.Workflow.Cart
{
    /// <summary>
    /// This activity handles processing different types of payments. It will call the appropriate 
    /// payment handler configured in the database and raise exceptions if something goes wrong.
    /// It also deals with removing sensitive data for credit card types of payments depending on the 
    /// configuration settings.
    /// </summary>
    public class ProcessPaymentActivity : CartActivityBase
    {
        private const string EventsCategory = "Handlers";

        // Define private constants for the Validation Errors 
        private const int TotalPaymentMismatch = 1;

        [NonSerialized]
        private readonly ILogger Logger;

        /// <summary>
        /// Gets or sets the payment.
        /// </summary>
        /// <value>The payment.</value>
        [ActivityFlowContextProperty]
        public Payment Payment { get; set; }

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
        /// Initializes a new instance of the <see cref="ProcessPaymentActivity"/> class.
        /// </summary>
        public ProcessPaymentActivity()
        {
            Logger = LogManager.GetLogger(GetType());
        }

        /// <summary>
        /// Executes the specified execution context.
        /// </summary>
        /// <param name="executionContext">The execution context.</param>
        /// <returns></returns>
        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            // Raise the ProcessingPaymentEvent event to the parent workflow
            RaiseEvent(ProcessingPaymentEvent, EventArgs.Empty);

            // Validate the properties at runtime
            ValidateRuntime();

            // Process payment now
            ProcessPayment();

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
            base.ValidateOrderProperties(validationErrors);

            if (Payment == null)
            {
                // Cycle through all Order Forms and check total, it should be equal to total of all payments
                decimal paymentTotal = 0;
                foreach (OrderForm orderForm in OrderGroup.OrderForms)
                {
                    foreach (Payment payment in orderForm.Payments)
                    {
                        paymentTotal += payment.Amount;
                    }
                }

                if (paymentTotal < OrderGroup.Total)
                {
                    Logger.Error(string.Format("Payment total is less than order total."));
                    var validationError = new ValidationError("The payment total and the order total do not not match. Please adjust your payment.", TotalPaymentMismatch, false);
                    validationErrors.Add(validationError);
                }
            }
        }

        /// <summary>
        /// Processes the payment.
        /// </summary>
        private void ProcessPayment()
        {
            // If total is 0, we do not need to proceed
            if (OrderGroup.Total == 0 || OrderGroup is PaymentPlan)
                return;

            // Start Charging!
            PaymentMethodDto methods = PaymentManager.GetPaymentMethods(/*Thread.CurrentThread.CurrentCulture.Name*/String.Empty);
            foreach (OrderForm orderForm in OrderGroup.OrderForms)
            {
                foreach (Payment payment in orderForm.Payments)
                {
                    if (Payment != null && !Payment.Equals(payment))
                        continue;

                    //Do not process payments with status Processing and Fail
                    var paymentStatus = PaymentStatusManager.GetPaymentStatus(payment);
                    if (paymentStatus != PaymentStatus.Pending)
                        continue;

                    PaymentMethodDto.PaymentMethodRow paymentMethod = methods.PaymentMethod.FindByPaymentMethodId(payment.PaymentMethodId);

                    // If we couldn't find payment method specified, generate an error
                    if (paymentMethod == null)
                    {
                        throw new MissingMethodException(String.Format("Specified payment method \"{0}\" has not been defined.", payment.PaymentMethodId));
                    }

                    Logger.Debug(String.Format("Getting the type \"{0}\".", paymentMethod.ClassName));
                    Type type = Type.GetType(paymentMethod.ClassName);
                    if (type == null)
                        throw new TypeLoadException(String.Format("Specified payment method class \"{0}\" can not be created.", paymentMethod.ClassName));

                    Logger.Debug(String.Format("Creating instance of \"{0}\".", type.Name));
                    IPaymentGateway provider = (IPaymentGateway)Activator.CreateInstance(type);

                    provider.Settings = CreateSettings(paymentMethod);

                    string message = "";
                    Logger.Debug(String.Format("Processing the payment."));

                    var processPaymentResult = false;
                    var splitPaymentProvider = provider as ISplitPaymentGateway;

                    try
                    {
                        if (splitPaymentProvider != null)
                        {
                            processPaymentResult = splitPaymentProvider.ProcessPayment(payment, Shipment, ref message);
                        }
                        else
                        {
                            processPaymentResult = provider.ProcessPayment(payment, ref message);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is PaymentException)
                        {
                            throw;
                        }

                        // throw a payment exception for all providers exception
                        throw new PaymentException(PaymentException.ErrorType.ProviderError, "", String.Format(ex.Message));
                    }

                    // update payment status
                    if (processPaymentResult)
                    {
                        Mediachase.Commerce.Orders.Managers.PaymentStatusManager.ProcessPayment(payment);
                    }
                    else
                    {
                        throw new PaymentException(PaymentException.ErrorType.ProviderError, "", String.Format(message));
                    }

                    Logger.Debug(String.Format("Payment processed."));
                }
            }
        }

        /// <summary>
        /// Creates the settings.
        /// </summary>
        /// <param name="methodRow">The method row.</param>
        /// <returns></returns>
        private Dictionary<string, string> CreateSettings(PaymentMethodDto.PaymentMethodRow methodRow)
        {
            Dictionary<string, string> settings = new Dictionary<string, string>();

            PaymentMethodDto.PaymentMethodParameterRow[] rows = methodRow.GetPaymentMethodParameterRows();
            foreach (PaymentMethodDto.PaymentMethodParameterRow row in rows)
            {
                settings.Add(row.Parameter, row.Value);
            }

            return settings;
        }

    }
}
