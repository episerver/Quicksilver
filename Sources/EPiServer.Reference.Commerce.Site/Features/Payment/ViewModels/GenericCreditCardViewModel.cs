using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;
using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels
{
    public class GenericCreditCardViewModel : PaymentMethodViewModel<GenericCreditCardPaymentMethod>
    {
        public GenericCreditCardViewModel()
        {
            InitializeValues();
        }

        public List<SelectListItem> Months { get; set; }

        public List<SelectListItem> Years { get; set; }

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