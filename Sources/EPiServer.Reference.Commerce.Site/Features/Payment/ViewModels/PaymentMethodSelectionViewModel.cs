using EPiServer.Commerce.Order;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels
{
    public class PaymentMethodSelectionViewModel
    {
        public IEnumerable<PaymentMethodViewModel<IPaymentMethod>> PaymentMethods { get; set; }

        public PaymentMethodViewModel<IPaymentMethod> SelectedPaymentMethod { get; set; }
    }
}