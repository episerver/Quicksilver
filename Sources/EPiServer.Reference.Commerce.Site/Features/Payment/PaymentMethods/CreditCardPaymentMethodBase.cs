using EPiServer.Commerce.Order;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods
{
    public abstract class CreditCardPaymentMethodBase : PaymentMethodBase, IDataErrorInfo
    {
        protected CreditCardPaymentMethodBase()
            : this(LocalizationService.Current, 
                  ServiceLocator.Current.GetInstance<IOrderGroupFactory>(),
                  ServiceLocator.Current.GetInstance<LanguageService>(),
                  ServiceLocator.Current.GetInstance<IPaymentManagerFacade>())
        {
        }

        protected CreditCardPaymentMethodBase(LocalizationService localizationService, 
            IOrderGroupFactory orderGroupFactory,
            LanguageService languageService, 
            IPaymentManagerFacade paymentManager) 
            : base(localizationService, 
                  orderGroupFactory,
                  languageService, 
                  paymentManager)
        {
            InitializeValues();
        }

        public List<SelectListItem> Months { get; set; }

        public List<SelectListItem> Years { get; set; }

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

        string IDataErrorInfo.Error => null;

        string IDataErrorInfo.this[string columnName] => GetValidationError(columnName);

        static readonly string[] ValidatedProperties =
{
            "CreditCardNumber",
            "CreditCardSecurityCode",
            "ExpirationYear",
            "ExpirationMonth",
        };
        
        public override bool ValidateData()
        {
            return ValidatedProperties.All(property => GetValidationError(property) == null);
        }

        public virtual void InitializeValues()
        {
            Months = new List<SelectListItem>();
            Years = new List<SelectListItem>();

            for (var i = 1; i < 13; i++)
            {
                Months.Add(new SelectListItem
                {
                    Text = i.ToString(CultureInfo.InvariantCulture),
                    Value = i.ToString(CultureInfo.InvariantCulture)
                });
            }

            for (var i = 0; i < 7; i++)
            {
                var year = (DateTime.Now.Year + i).ToString(CultureInfo.InvariantCulture);
                Years.Add(new SelectListItem
                {
                    Text = year,
                    Value = year
                });
            }
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
            }

            return error;
        }

        protected virtual string ValidateExpirationMonth()
        {
            if (ExpirationYear == DateTime.Now.Year && ExpirationMonth < DateTime.Now.Month)
            {
                return LocalizationService.GetString("/Checkout/Payment/Methods/CreditCard/ValidationErrors/ExpirationMonth");
            }

            return null;
        }

        protected virtual string ValidateExpirationYear()
        {
            return ExpirationYear < DateTime.Now.Year ? 
                LocalizationService.GetString("/Checkout/Payment/Methods/CreditCard/ValidationErrors/ExpirationYear") : 
                null;
        }

        protected virtual string ValidateCreditCardSecurityCode()
        {
            if (string.IsNullOrEmpty(CreditCardSecurityCode))
            {
                return LocalizationService.GetString("/Checkout/Payment/Methods/CreditCard/Empty/CreditCardSecurityCode");
            }

            return !Regex.IsMatch(CreditCardSecurityCode, "^[0-9]{3}$") ? 
                LocalizationService.GetString("/Checkout/Payment/Methods/CreditCard/ValidationErrors/CreditCardSecurityCode") : 
                null;
        }

        protected virtual string ValidateCreditCardNumber()
        {
            return string.IsNullOrEmpty(CreditCardNumber) ? 
                LocalizationService.GetString("/Checkout/Payment/Methods/CreditCard/Empty/CreditCardNumber") : 
                null;
        }
    }
}