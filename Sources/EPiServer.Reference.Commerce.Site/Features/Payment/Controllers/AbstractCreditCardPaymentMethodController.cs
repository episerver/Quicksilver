using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Payment.Models;
using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Web.Routing;
using Mediachase.Commerce.Website.Helpers;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.Controllers
{
    public class AbstractCreditCardPaymentMethodController : BasePaymentController<AbstractCreditCardPaymentMethodFormModel>
    {
        private readonly IContentLoader _contentLoader;
        private readonly Func<CartHelper> _cartHelper;
        private readonly UrlResolver _urlResolver;

        public AbstractCreditCardPaymentMethodController(IContentLoader contentLoader,
                                               Func<CartHelper> cartHelper,
                                               UrlResolver urlResolver)
        {
            _contentLoader = contentLoader;
            _cartHelper = cartHelper;
            _urlResolver = urlResolver;
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult PaymentMethod(Guid paymentMethodId, string paymentMethodSystemName, AbstractCreditCardPaymentViewModel paymentModel)
        {
            var formModel = new AbstractCreditCardPaymentMethodFormModel
                {
                    PaymentViewModel =
                        paymentModel ?? new AbstractCreditCardPaymentViewModel
                            {
                                ExpirationMonth = DateTime.Now.Month,
                                ExpirationYear = DateTime.Now.Year,
                                CardType = "MasterCard",
                                CreditCardNumber = "5555555555554444",
                                CreditCardSecurityCode = "123"
                            }
                };


            var model = new AbstractCreditCardPaymentMethodViewModel
                {
                    FormModel = formModel,
                    Months = new List<SelectListItem>(),
                    Years = new List<SelectListItem>()
                };

            for (var i = 1; i < 13; i++)
            {
                model.Months.Add(new SelectListItem
                    {
                        Text = i.ToString(CultureInfo.InvariantCulture),
                        Value = i.ToString(CultureInfo.InvariantCulture)
                    });
            }

            for (var i = 0; i < 7; i++)
            {
                var year = (DateTime.Now.Year + i).ToString(CultureInfo.InvariantCulture);
                model.Years.Add(new SelectListItem
                    {
                        Text = year,
                        Value = year
                    });
            }


            model.Id = paymentMethodId;
            model.SystemName = paymentMethodSystemName;
            return PartialView("AbstractCreditCardPaymentMethod", model);
        }

        [HttpPost]
        public override ActionResult Process()
        {
            var cart = _cartHelper().Cart;
            if (!cart.OrderForms.Any())
            {
                cart.OrderForms.AddNew();
            }

            var authorizePaymentOption = new AbstractCreditCardPaymentOption(PaymentModel.PaymentViewModel);
            var payment = authorizePaymentOption.PreProcess(cart.OrderForms[0]);
            if (payment != null)
            {
                cart.OrderForms[0].Payments.Add(payment);
                cart.AcceptChanges();
            }
            else
            {
                throw new Exception("Payment error");
            }

            var startPage = _contentLoader.Get<StartPage>(ContentReference.StartPage);
            var url = authorizePaymentOption.PostProcess(cart.OrderForms[0])
                          ? _urlResolver.GetUrl(startPage.CheckoutPage, startPage.Language.Name, new VirtualPathArguments {Action = "Finish"})
                          : _urlResolver.GetUrl(startPage.CheckoutPage);

            return Redirect(url);
        }
    }
}