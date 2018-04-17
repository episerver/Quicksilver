using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Payment.ViewModelFactories;
using System;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.Controllers
{
    public class PaymentController : Controller
    {
        private readonly PaymentMethodViewModelFactory _paymentMethodViewModelFactory;

        public PaymentController(PaymentMethodViewModelFactory paymentMethodViewModelFactory)
        {
            _paymentMethodViewModelFactory = paymentMethodViewModelFactory;
        }

        [HttpPost]
        public PartialViewResult SetPaymentMethod(Guid paymentMethodId)
        {
            var viewModel = _paymentMethodViewModelFactory.CreatePaymentMethodSelectionViewModel(paymentMethodId);

            return PartialView("_PaymentMethodSelection", viewModel);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public PartialViewResult PaymentMethodSelection(IPaymentMethod payment)
        {
            var viewModel = _paymentMethodViewModelFactory.CreatePaymentMethodSelectionViewModel(payment);

            return PartialView("_PaymentMethodSelection", viewModel);
        }
    }
}