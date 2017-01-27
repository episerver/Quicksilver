using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Workflow.ReturnForm;
using Mediachase.Commerce.WorkflowCompatibility;
using System;

namespace Mediachase.Commerce.Workflow.ReturnFormActivities
{
    public class CreateExchangePaymentActivity : ReturnFormBaseActivity
    {
        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            CreateExchangePayments();
            // Retun the closed status indicating that this activity is complete.
            return ActivityExecutionStatus.Closed;
        }

        public void CreateExchangePayments()
        {
            var origPurchaseOrder = ReturnOrderForm.Parent as PurchaseOrder;
            var origOrderForm = origPurchaseOrder.OrderForms[0];
            var exchangeOrder = ReturnExchangeManager.GetExchangeOrderForReturnForm(ReturnOrderForm);
            if (exchangeOrder != null)
            {
                var exchangeOrderForm = exchangeOrder.OrderForms[0];
                //Exchange payments
                //Credit exchange payment to original order
                decimal paymentTotal = Math.Min(ReturnOrderForm.Total, exchangeOrder.Total);
                ExchangePayment creditExchangePayment = CreateExchangePayment(TransactionType.Credit, paymentTotal);
                origOrderForm.Payments.Add(creditExchangePayment);

                //Debit exchange payment to exchange order
                ExchangePayment debitExchangePayment = CreateExchangePayment(TransactionType.Capture, paymentTotal);
                exchangeOrderForm.Payments.Add(debitExchangePayment);

                OrderStatusManager.RecalculatePurchaseOrder(exchangeOrder);
                exchangeOrder.AcceptChanges();
            }

        }

        private static ExchangePayment CreateExchangePayment(TransactionType tranType, decimal amount)
        {
            ExchangePayment retVal = new ExchangePayment();
            PaymentMethodDto paymentMethods = PaymentManager.GetPaymentMethods("en", true);

            foreach (PaymentMethodDto.PaymentMethodRow row in paymentMethods.PaymentMethod.Rows)
            {
                if (row.SystemKeyword == ExchangePayment.PaymentMethodSystemKeyword)
                {
                    retVal.PaymentMethodId = row.PaymentMethodId;
                    retVal.PaymentMethodName = row.Name;
                    break;
                }
            }

            retVal.Amount = amount;
            retVal.TransactionType = tranType.ToString();
            retVal.Status = PaymentStatus.Processed.ToString();

            return retVal;
        }
    }
}