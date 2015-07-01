using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.Models
{
    public class GenericCreditCardViewModel : PaymentMethodViewModel<GenericCreditCardPaymentMethod>
    {
        public GenericCreditCardViewModel()
        {
            InitializeValues();
        }

        /// <summary>
        /// Gets or sets the available months to be used as credit card expiration date.
        /// </summary>
        public List<SelectListItem> Months { get; set; }

        /// <summary>
        /// Gets or sets the available years to be used as credit card expiration date.
        /// </summary>
        public List<SelectListItem> Years { get; set; }

        /// <summary>
        /// Adds months and years to the collections of possible values.
        /// </summary>
        public void InitializeValues()
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
    }
}