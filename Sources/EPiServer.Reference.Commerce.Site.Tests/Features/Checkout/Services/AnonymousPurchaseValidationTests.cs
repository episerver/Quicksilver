using System;
using System.Collections.Generic;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using Xunit;
using ModelStateDictionary = System.Web.Mvc.ModelStateDictionary;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Checkout.Services
{
    public class AnonymousPurchaseValidationTests
    {
        [Fact]
        public void ValidateModel_WhenModelIsValid_ShouldSucceedValidation()
        {
            var modelState = new ModelStateDictionary();
            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel
                {
                    Email = "someone@example.com"
                },
                Shipments = new List<ShipmentViewModel>
                {
                    new ShipmentViewModel
                    {
                        Address = new AddressModel(),
                        ShippingMethodId = Guid.NewGuid()
                    }
                }
            };

            var result = _subject.ValidateModel(modelState, viewModel);
            
            Assert.True(modelState.IsValid);
            Assert.True(result);
        }

        [Fact]
        public void ValidateModel_WhenBillingAddressIsInvalid_ShouldFailValidation()
        {
            var modelState = new ModelStateDictionary();
            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel
                {
                    Email = ""
                },
                Shipments = new List<ShipmentViewModel>
                {
                    new ShipmentViewModel
                    {
                        Address = new AddressModel(),
                        ShippingMethodId = Guid.NewGuid()
                    }
                }
            };

            var result = _subject.ValidateModel(modelState, viewModel);

            Assert.False(result);
            Assert.False(modelState.IsValid);

        }

        [Fact]
        public void ValidateModel_WhenShippingMethodsIsInvalid_ShouldFailValidation()
        {
            var modelState = new ModelStateDictionary();
            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel
                {
                    Email = "someone@something.com"
                },
                Shipments = new List<ShipmentViewModel>
                {
                    new ShipmentViewModel
                    {
                        Address = new AddressModel(),
                    }
                }
            };

            var result = _subject.ValidateModel(modelState, viewModel);

            Assert.False(modelState.IsValid);
            Assert.False(result);
        }

        private readonly AnonymousPurchaseValidation _subject;

        public AnonymousPurchaseValidationTests()
        {
            _subject = new AnonymousPurchaseValidation(new MemoryLocalizationService());
        }
    }
}
