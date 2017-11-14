using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
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
                BillingAddress = new AddressModel { AddressId = SampleAddressId, Email = SampleEmail },
                Shipments = new List<ShipmentViewModel>
                {
                    new ShipmentViewModel
                    {
                        Address = new AddressModel { AddressId = SampleAddressId},
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
                BillingAddress = new AddressModel { AddressId = SampleAddressId, Email = SampleEmail },
                Shipments = new List<ShipmentViewModel>
                {
                    new ShipmentViewModel
                    {
                        Address = new AddressModel { AddressId = SampleAddressId},
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
                BillingAddress = new AddressModel { AddressId = SampleAddressId, Email = SampleEmail },
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
        public void ValidateModel_WhenBillingAddressIdIsEmpty_ShouldFailValidation()
        {
            var modelState = new ModelStateDictionary();
            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel() { Email = SampleEmail },
                Shipments = new List<ShipmentViewModel>
                {
                    new ShipmentViewModel
                    {
                        Address = new AddressModel { AddressId = SampleAddressId},
                        ShippingMethodId = Guid.NewGuid()
                    }
                }
            };

            var result = _subject.ValidateModel(modelState, viewModel);

            Assert.False(modelState.IsValid);
            Assert.False(result);
        }

        [Fact]
        public void ValidateModel_WhenBillingAddressEmailIsEmpty_ShouldFailValidation()
        {
            var modelState = new ModelStateDictionary();
            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel { AddressId = SampleAddressId },
                Shipments = new List<ShipmentViewModel>
                {
                    new ShipmentViewModel
                    {
                        Address = new AddressModel { AddressId = SampleAddressId},
                        ShippingMethodId = Guid.NewGuid()
                    }
                }
            };

            var result = _subject.ValidateModel(modelState, viewModel);

            Assert.False(modelState.IsValid);
            Assert.False(result);
        }

        private readonly AuthenticatedPurchaseValidation _subject;
        private const string SampleAddressId = "addressId";
        private const string SampleEmail = "email@sample.com";

        public AuthenticatedPurchaseValidationTests()
        {
            _subject = new AuthenticatedPurchaseValidation(new MemoryLocalizationService());
        }
    }
}
