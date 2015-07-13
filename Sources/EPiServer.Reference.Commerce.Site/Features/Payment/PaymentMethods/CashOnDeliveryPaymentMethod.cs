using EPiServer.Framework.Localization;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Website;
using System;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods
{
    public class CashOnDeliveryPaymentMethod : PaymentMethodBase, IPaymentOption
    {
        public CashOnDeliveryPaymentMethod()
            : this(LocalizationService.Current)
        {
        }

        public CashOnDeliveryPaymentMethod(LocalizationService localizationService)
            : base(localizationService)
        {   
        }

        public bool ValidateData()
        {
            return true;
        }

        public Mediachase.Commerce.Orders.Payment PreProcess(OrderForm form)
        {
            if (form == null)
            {
                throw new ArgumentNullException("form");
            }

            var payment = new OtherPayment
            {
                PaymentMethodId = PaymentMethodId,
                PaymentMethodName = "CashOnDelivery",
                OrderFormId = form.OrderFormId,
                OrderGroupId = form.OrderGroupId,
                Amount = form.Total,
                Status = PaymentStatus.Pending.ToString(),
                TransactionType = TransactionType.Authorization.ToString()
            };

            return payment;
        }

        public bool PostProcess(OrderForm form)
        {
            if (form == null)
            {
                throw new ArgumentNullException("form");
            }

            var payment = form.Payments.ToArray().FirstOrDefault(x => x.PaymentMethodId == this.PaymentMethodId);
            if (payment == null)
            { 
                return false;
            }

            payment.Status = PaymentStatus.Processed.ToString();
            payment.AcceptChanges();

            return true;
        }
    }
}