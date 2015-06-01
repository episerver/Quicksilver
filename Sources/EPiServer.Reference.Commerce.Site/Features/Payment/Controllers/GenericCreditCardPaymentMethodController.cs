using EPiServer.Reference.Commerce.Site.Features.Payment.Models;
using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;
using System;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.Controllers
{
    public class GenericCreditCardPaymentMethodController : Controller
    {
        /// <summary>
        /// Renders the partial view for a credit card payment.
        /// </summary>
        /// <param name="paymentMethodId">The id of the payment method.</param>
        /// <param name="paymentMethodSystemName">The system name of the payment method.</param>
        /// <param name="viewModel">An existing view model.</param>
        /// <returns>The pertial view for the GenericCreditCardPaymentMethod.</returns>
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult PaymentMethod(Guid paymentMethodId, string paymentMethodSystemName, GenericCreditCardPaymentMethodViewModel viewModel)
        {
            if (viewModel == null)
            {
                viewModel = new GenericCreditCardPaymentMethodViewModel
                {
                    PaymentMethod = new GenericCreditCardPaymentMethod
                    {
                        ExpirationMonth = DateTime.Now.Month,
                        ExpirationYear = DateTime.Now.Year,
                        CardType = "MasterCard",
                        CreditCardNumber = "5555555555554444",
                        CreditCardSecurityCode = "123",
                        PaymentMethodId = paymentMethodId
                    }
                };
            }

            viewModel.InitializeValues();
            viewModel.Id = paymentMethodId;
            viewModel.SystemName = paymentMethodSystemName;

            return PartialView("GenericCreditCardPaymentMethod", viewModel);
        }
    }
}