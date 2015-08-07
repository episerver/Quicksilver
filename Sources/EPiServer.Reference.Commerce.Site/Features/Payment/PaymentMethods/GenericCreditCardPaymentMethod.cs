using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Website;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods
{
    public class GenericCreditCardPaymentMethod : PaymentMethodBase, IPaymentOption, IDataErrorInfo
    {
        static readonly string[] ValidatedProperties = 
        {
            "CreditCardNumber",
            "CreditCardSecurityCode",
            "ExpirationYear",
            "ExpirationMonth",
        };

        public GenericCreditCardPaymentMethod()
            : this(LocalizationService.Current)
        {
        }

        public GenericCreditCardPaymentMethod(LocalizationService localizationService)
            : base(localizationService)
        {
            ExpirationMonth = DateTime.Now.Month;
            CreditCardSecurityCode = "212";
            CardType = "Generic";
            CreditCardNumber = "4662519843660534";
        }

        [LocalizedDisplay("/Checkout/Payment/Methods/CreditCard/Labels/CreditCardName")]
        [LocalizedRequired("/Checkout/Payment/Methods/CreditCard/Empty/CreditCardName")]
        public string CreditCardName { get; set; }

        [LocalizedDisplay("/Checkout/Payment/Methods/CreditCard/Labels/CreditCardNumber")]
        [LocalizedRequired("/Checkout/Payment/Methods/CreditCard/Empty/CreditCardNumber")]
        public string CreditCardNumber { get; set; }

        [LocalizedDisplay("/Checkout/Payment/Methods/CreditCard/Labels/CreditCardSecurityCode")]
        [LocalizedRequired("/Checkout/Payment/Methods/CreditCard/Empty/CreditCardSecurityCode")]
        public string CreditCardSecurityCode { get; set; }

        [LocalizedDisplay("/Checkout/Payment/Methods/CreditCard/Labels/ExpirationMonth")]
        [LocalizedRequired("/Checkout/Payment/Methods/CreditCard/Empty/ExpirationMonth")]
        public int ExpirationMonth { get; set; }

        [LocalizedDisplay("/Checkout/Payment/Methods/CreditCard/Labels/ExpirationYear")]
        [LocalizedRequired("/Checkout/Payment/Methods/CreditCard/Empty/ExpirationYear")]
        public int ExpirationYear { get; set; }

        public string CardType { get; set; }

        public bool IsValid
        {
            get
            {
                foreach (string property in ValidatedProperties)
                {
                    if (GetValidationError(property) != null)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        string IDataErrorInfo.Error
        {
            get { return null; }
        }

        string IDataErrorInfo.this[string propertyName]
        {
            get { return this.GetValidationError(propertyName); }
        }

        private string GetValidationError(string property)
        {
            string error = null;

            switch (property)
            {
                case "CreditCardNumber":
                    error = ValidateCreditCardNumber();
                    break;

                case "CreditCardSecurityCode":
                    error = ValidateCreditCardSecurityCode();
                    break;

                case "ExpirationYear":
                    error = ValidateExpirationYear();
                    break;

                case "ExpirationMonth":
                    error = ValidateExpirationMonth();
                    break;

                default:
                    break;
            }

            return error;
        }

        private string ValidateExpirationMonth()
        {
            if (ExpirationYear == DateTime.Now.Year && ExpirationMonth < DateTime.Now.Month)
            {
                return _localizationService.GetString("/Checkout/Payment/Methods/CreditCard/ValidationErrors/ExpirationMonth");
            }

            return null;
        }

        private string ValidateExpirationYear()
        {
            if (ExpirationYear < DateTime.Now.Year)
            {
                return _localizationService.GetString("/Checkout/Payment/Methods/CreditCard/ValidationErrors/ExpirationYear");
            }

            return null;
        }

        private string ValidateCreditCardSecurityCode()
        {
            if (string.IsNullOrEmpty(CreditCardSecurityCode))
            {
                return _localizationService.GetString("/Checkout/Payment/Methods/CreditCard/Empty/CreditCardSecurityCode");
            }

            if (!Regex.IsMatch(CreditCardSecurityCode, "^[0-9]{3}$"))
            {
                return _localizationService.GetString("/Checkout/Payment/Methods/CreditCard/ValidationErrors/CreditCardSecurityCode");
            }

            return null;
        }

        private string ValidateCreditCardNumber()
        {
            if (string.IsNullOrEmpty(CreditCardNumber))
            {
                return _localizationService.GetString("/Checkout/Payment/Methods/CreditCard/Empty/CreditCardNumber");
            }

            if (CreditCardNumber[CreditCardNumber.Length - 1] != '4')
            {
                return _localizationService.GetString("/Checkout/Payment/Methods/CreditCard/ValidationErrors/CreditCardNumber");
            }

            return null;
        }

        public bool ValidateData()
        {
            return IsValid;
        }

        public Mediachase.Commerce.Orders.Payment PreProcess(OrderForm orderForm)
        {
            if (orderForm == null) throw new ArgumentNullException("orderForm");

            if (!ValidateData())
                return null;

            var payment = new CreditCardPayment
                {
                    CardType = "Credit card",
                    PaymentMethodId = PaymentMethodId,
                    PaymentMethodName = "GenericCreditCard",
                    OrderFormId = orderForm.OrderFormId,
                    OrderGroupId = orderForm.OrderGroupId,
                    Amount = orderForm.Total,
                    CreditCardNumber = CreditCardNumber,
                    CreditCardSecurityCode = CreditCardSecurityCode,
                    ExpirationMonth = ExpirationMonth,
                    ExpirationYear = ExpirationYear,
                    Status = PaymentStatus.Pending.ToString(),
                    CustomerName = CreditCardName,
                    TransactionType = TransactionType.Authorization.ToString()
            };

            return payment;
        }

        public bool PostProcess(OrderForm orderForm)
        {
            var card = orderForm.Payments.ToArray().FirstOrDefault(x => x.PaymentType == PaymentType.CreditCard);
            if (card == null)
                return false;

            card.Status = PaymentStatus.Processed.ToString();
            card.AcceptChanges();
            return true;
        }
    }
}