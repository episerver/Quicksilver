using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPiServer.Reference.Commerce.Site.Features.Payment.Models;
using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;
using EPiServer.Reference.Commerce.Site.Features.Payment.Controllers;
using System.Web.Mvc;
using FluentAssertions;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Payment.Controllers
{
    [TestClass]
    public class GenericCreditCardPaymentMethodControllerTests
    {
        [TestMethod]
        public void PaymentMethod_WhenPassingViewModel_ShouldCreateEquivalentModel()
        {
            var paymentMethodId = Guid.NewGuid();
            var expectedModel = CreateExpectedModel(paymentMethodId,"mySystemName", DateTime.Now.Month, DateTime.Now.Year, "MyCreditCard", "55555555555555", "123");
            var subject = new GenericCreditCardPaymentMethodController();
            var result = ((PartialViewResult)subject.PaymentMethod(Guid.NewGuid(), "Whatever", expectedModel)).Model as GenericCreditCardPaymentMethodViewModel;
            result.ShouldBeEquivalentTo(expectedModel);
        }
        
        [TestMethod]
        public void PaymentMethod_ShouldCreateModelWithSystemNameEqualsToParameterPassed()
        {
            var paymentMethodId = Guid.NewGuid();
            var expectedModel = CreateExpectedModel(paymentMethodId, "mySystemName", DateTime.Now.Month, DateTime.Now.Year, "MyCreditCard", "55555555555555", "123");
            var subject = new GenericCreditCardPaymentMethodController();
            var result = ((PartialViewResult)subject.PaymentMethod(Guid.NewGuid(), "WhatEver", expectedModel)).Model as GenericCreditCardPaymentMethodViewModel;
            Assert.AreEqual<string>(result.SystemName, "WhatEver");
        }

        [TestMethod]
        public void PaymentMethod_WhenViewModelIsNull_ShouldCreateGenericModel()
        {
            var paymentMethodId = Guid.NewGuid();
            var expectedGenericModel = CreateExpectedModel(paymentMethodId, "MyGenericSystemName", DateTime.Now.Month, DateTime.Now.Year, "MasterCard", "5555555555554444", "123");
            var subject = new GenericCreditCardPaymentMethodController();
            var result = ((PartialViewResult)subject.PaymentMethod(paymentMethodId, "MyGenericSystemName", null)).Model as GenericCreditCardPaymentMethodViewModel;

            result.ShouldBeEquivalentTo(expectedGenericModel);
        }

        private GenericCreditCardPaymentMethodViewModel CreateExpectedModel(Guid paymentMethodId, string systemName, int expirationMonth, int expirationYear, string cardType,
                                                                                string creditCardNumber, string creditCardSecurityCode)
        {
            return new GenericCreditCardPaymentMethodViewModel
            {
                PaymentMethod = new GenericCreditCardPaymentMethod
                {
                    ExpirationMonth = expirationMonth,
                    ExpirationYear = expirationYear,
                    CardType = cardType,
                    CreditCardNumber = creditCardNumber,
                    CreditCardSecurityCode = creditCardSecurityCode,
                    PaymentMethodId = paymentMethodId
                },
                Id = paymentMethodId,
                SystemName = systemName
            };
        }
    }

}
