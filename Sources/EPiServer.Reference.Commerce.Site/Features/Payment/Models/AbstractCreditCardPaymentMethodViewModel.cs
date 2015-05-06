using System.Collections.Generic;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.Models
{
    public class AbstractCreditCardPaymentMethodViewModel : PaymentMethodViewModel
    {
        public List<SelectListItem> Months { get; set; }
        public List<SelectListItem> Years { get; set; }

        public AbstractCreditCardPaymentMethodFormModel FormModel { get; set; }
    }
}