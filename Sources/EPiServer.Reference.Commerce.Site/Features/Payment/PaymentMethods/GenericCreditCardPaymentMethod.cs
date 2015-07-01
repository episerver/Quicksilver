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
    /// <summary>
    /// Payment method used as part of a credit card purchase.
    /// </summary>
    public class GenericCreditCardPaymentMethod : PaymentMethodBase, IPaymentOption, IDataErrorInfo
    {
        /// <summary>
        /// A collection of all properties that has custom validation logic.
        /// </summary>
        static readonly string[] ValidatedProperties = 
        {
            "CreditCardNumber",
            "CreditCardSecurityCode",
            "ExpirationYear",
            "ExpirationMonth",
        };

        /// <summary>
        /// Returns a new instance of an GenericCreditCardPaymentMethod.
        /// </summary>
        public GenericCreditCardPaymentMethod()
            : this(LocalizationService.Current)
        {
        }

        /// <summary>
        /// Returns a new instance of an GenericCreditCardPaymentMethod taking an existing LocalizationService as
        /// argument.
        /// </summary>
        /// <param name="localizationService">LocalizationService used for translating error messages.</param>
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

        /// <summary>
        /// Gets or sets tge CardType.
        /// </summary>
        public string CardType { get; set; }

        /// <summary>
        /// Gets whether this payment method is valid or not.
        /// </summary>
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

        /// <summary>
        /// Gets any existing error messages for a certain property.
        /// </summary>
        /// <param name="propertyName">The property to inspect.</param>
        /// <returns>Null if the property is valid. In case of errors the error message is returned.</returns>
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

        /// <summary>
        /// Validates and returns any existing errors of the ExpirationMonth.
        /// </summary>
        /// <returns>Any existing errors of the ExpirationDate property. If property is valid then the returned value is null.</returns>
        private string ValidateExpirationMonth()
        {
            if (ExpirationYear == DateTime.Now.Year && ExpirationMonth < DateTime.Now.Month)
            {
                return _localizationService.GetString("/Checkout/Payment/Methods/CreditCard/ValidationErrors/ExpirationMonth");
            }

            return null;
        }

        /// <summary>
        /// Validates and returns any existing errors of the ExpirationYear.
        /// </summary>
        /// <returns>Any existing errors of the ExpirationYear property. If property is valid then the returned value is null.</returns>
        private string ValidateExpirationYear()
        {
            if (ExpirationYear < DateTime.Now.Year)
            {
                return _localizationService.GetString("/Checkout/Payment/Methods/CreditCard/ValidationErrors/ExpirationYear");
            }

            return null;
        }

        /// <summary>
        /// Validates and returns any existing errors of the CreditCardSecurityCode.
        /// </summary>
        /// <returns>Any existing errors of the CreditCardSecurityCode property. If property is valid then the returned value is null.</returns>
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

        /// <summary>
        /// Validates and returns any existing errors of the CreditCardNumber.
        /// </summary>
        /// <returns>Any existing errors of the CreditCardNumber property. If property is valid then the returned value is null.</returns>
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