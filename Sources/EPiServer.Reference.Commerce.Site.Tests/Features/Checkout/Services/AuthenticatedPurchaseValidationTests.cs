using System;
using System.Collections.Generic;
using System.Web.Mvc;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Checkout.Services
{
    public class AuthenticatedPurchaseValidationTests
    {
        [Fact]
        public void ValidateModel_WhenModelIsValid_ShouldSucceedValidation()
        {
            var modelState = new ModelStateDictionary();
            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel { AddressId = "addressId" },
                Shipments = new List<ShipmentViewModel>
                {
                    new ShipmentViewModel
                    {
                        Address = new AddressModel { AddressId = "addressId"},
                        ShippingMethodId = Guid.NewGuid()
                    }
                }
            };

            var result = _subject.ValidateModel(modelState, viewModel);

            Assert.True(modelState.IsValid);
            Assert.True(result);
        }

        [Fact]
        public void ValidateModel_WhenShippingMethodIsInvalid_ShouldFailValidation()
        {
            var modelState = new ModelStateDictionary();
            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel { AddressId = "addressId" },
                Shipments = new List<ShipmentViewModel>
                {
                    new ShipmentViewModel
                    {
                        Address = new AddressModel { AddressId = "addressId"},
                    }
                }
            };

            var result = _subject.ValidateModel(modelState, viewModel);

            Assert.False(modelState.IsValid);
            Assert.False(result);
        }

        [Fact]
        public void ValidateModel_WhenShippingAddressIsInvalid_ShouldFailValidation()
        {
            var modelState = new ModelStateDictionary();
            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel { AddressId = "addressId" },
                Shipments = new List<ShipmentViewModel>
                {
                    new ShipmentViewModel
                    {
                        Address = new AddressModel (),
                        ShippingMethodId = Guid.NewGuid()
                    }
                }
            };

            var result = _subject.ValidateModel(modelState, viewModel);

            Assert.False(modelState.IsValid);
            Assert.False(result);
        }

        [Fact]
        public void ValidateModel_WhenBillingAddressIsInvalid_ShouldFailValidation()
        {
            var modelState = new ModelStateDictionary();
            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel (),
                Shipments = new List<ShipmentViewModel>
                {
                    new ShipmentViewModel
                    {
                        Address = new AddressModel { AddressId = "addressId"},
                        ShippingMethodId = Guid.NewGuid()
                    }
                }
            };

            var result = _subject.ValidateModel(modelState, viewModel);

            Assert.False(modelState.IsValid);
            Assert.False(result);
        }

        private readonly AuthenticatedPurchaseValidation _subject;

        public AuthenticatedPurchaseValidationTests()
        {
            _subject = new AuthenticatedPurchaseValidation(new MemoryLocalizationService());
        }
    }
}
