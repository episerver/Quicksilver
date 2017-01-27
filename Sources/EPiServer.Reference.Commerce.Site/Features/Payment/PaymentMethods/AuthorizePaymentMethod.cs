using EPiServer.Commerce.Order;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Orders;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods
{
    public class AuthorizePaymentMethod : PaymentMethodBase, IDataErrorInfo
    {
        static readonly string[] ValidatedProperties =
        {
            "CreditCardNumber",
            "CreditCardSecurityCode",
            "ExpirationYear",
            "ExpirationMonth",
        };

        public AuthorizePaymentMethod()
        : this(LocalizationService.Current, ServiceLocator.Current.GetInstance<IOrderGroupFactory>())
        {
        }

        public AuthorizePaymentMethod(LocalizationService localizationService, IOrderGroupFactory orderGroupFactory) : base(localizationService, orderGroupFactory)
        {
            ExpirationMonth = DateTime.Now.AddMonths(1).Month;
            ExpirationYear = DateTime.Now.AddMonths(1).Year;
            CreditCardSecurityCode = "027";
            CreditCardNumber = "4007000000027";
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

        public string Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {
            get { return GetValidationError(columnName); }
        }

        public override IPayment CreatePayment(decimal amount, IOrderGroup orderGroup)
        {
            var payment = orderGroup.CreateCardPayment(_orderGroupFactory);
            payment.CardType = "Credit card";
            payment.PaymentMethodId = PaymentMethodId;
            payment.PaymentMethodName = "Authorize";
            payment.Amount = amount;
            payment.CreditCardNumber = CreditCardNumber;
            payment.CreditCardSecurityCode = CreditCardSecurityCode;
            payment.ExpirationMonth = ExpirationMonth;
            payment.ExpirationYear = ExpirationYear;
            payment.Status = PaymentStatus.Pending.ToString();
            payment.CustomerName = CreditCardName;
            return payment;
        }

        public override void PostProcess(IPayment payment)
        {
            var creditCardPayment = (ICreditCardPayment)payment;
            var visibleDigits = 4;
            var cardNumberLength = creditCardPayment.CreditCardNumber.Length;
            creditCardPayment.CreditCardNumber = new string('*', cardNumberLength - visibleDigits)
                + creditCardPayment.CreditCardNumber.Substring(cardNumberLength - visibleDigits, visibleDigits);
        }

        public override bool ValidateData()
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

            return null;
        }
    }
}