using System.Collections.Generic;
using System.Linq;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using Moq;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Checkout.Services
{
    public class CheckoutAddressHandlingTests
    {
        [Fact]
        public void UpdateUserAddresses_ForAuthenticatedUser_ShouldLoadAddressesFromAddressBook()
        {
            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel(),
                Shipments = new List<ShipmentViewModel>
                {
                    new ShipmentViewModel {Address = new AddressModel()}
                },
                IsAuthenticated = true
            };

            _subject.UpdateUserAddresses(viewModel);

            _addressBookServiceMock.Verify(x => x.LoadAddress(viewModel.BillingAddress), Times.Once);
            _addressBookServiceMock.Verify(x => x.LoadAddress(viewModel.Shipments.Single().Address), Times.Once);
        }

        [Fact]
        public void UpdateUserAddresses_ForAuthenticatedUser_WhenUseBillingAddressForShipmentIsFalse_ShouldNotSetShippingAddressAsBillingAddress()
        {
            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel(),
                Shipments = new List<ShipmentViewModel>
                {
                     new ShipmentViewModel {Address = new AddressModel()}
                },
                IsAuthenticated = true
            };

            _subject.UpdateUserAddresses(viewModel);

            Assert.NotEqual(viewModel.BillingAddress, viewModel.Shipments.Single().Address);
        }

        [Fact]
        public void UpdateUserAddresses_ForAnonymousUser_WhenUseBillingAddressForShipmentIsTrue_ShouldSetShippingAddressAsBillingAddress()
        {
            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel(),
                Shipments = new List<ShipmentViewModel>
                {
                     new ShipmentViewModel {Address = new AddressModel()}
                },
                UseBillingAddressForShipment = true,
                IsAuthenticated = false
            };

            _subject.UpdateUserAddresses(viewModel);

            Assert.Equal(viewModel.BillingAddress, viewModel.Shipments.Single().Address);
        }

        [Fact]
        public void UpdateUserAddresses_ForAnonymousUser_WhenUseBillingAddressForShipmentIsFalse_ShouldNotSetShippingAddressAsBillingAddress()
        {
            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel(),
                Shipments = new List<ShipmentViewModel>
                {
                     new ShipmentViewModel {Address = new AddressModel()}
                },
                IsAuthenticated = false
            };

            _subject.UpdateUserAddresses(viewModel);

            Assert.NotEqual(viewModel.BillingAddress, viewModel.Shipments.Single().Address);
        }

        [Fact]
        public void UpdateUserAddresses_ForAuthenticatedUser_WhenUseBillingAddressForShipmentIsTrue_ShouldSetShippingAddressAsBillingAddress()
        {
            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel(),
                Shipments = new List<ShipmentViewModel>
                {
                     new ShipmentViewModel {Address = new AddressModel()}
                },
                UseBillingAddressForShipment = true,
                IsAuthenticated = true
            };

            _subject.UpdateUserAddresses(viewModel);

            Assert.Equal(viewModel.BillingAddress, viewModel.Shipments.Single().Address);
        }

        [Fact]
        public void ChangeAddress_WhenShippingAddressIndexHasValidValue_ShouldUpdateBothBillingAddressAndShippingAddress()
        {
            var shipmentModels = new List<ShipmentViewModel>
            {
                new ShipmentViewModel {Address = new AddressModel()}
            };
            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel(),
                Shipments = shipmentModels
            };
            var updateModel = new UpdateAddressViewModel
            {
                BillingAddress = new AddressModel { AddressId = "BillingAddId" },
                Shipments = shipmentModels,
                ShippingAddressIndex = 0
            };
            updateModel.Shipments[0].Address.AddressId = "ShipmentAddId";

            _subject.ChangeAddress(viewModel, updateModel);

            Assert.Equal("BillingAddId", viewModel.BillingAddress.AddressId);
            Assert.Equal("ShipmentAddId", viewModel.Shipments[0].Address.AddressId);
        }

        [Fact]
        public void ChangeAddress_WhenShippingAddressIndexHasInValidValue_ShouldUpdateBillingAddressButNotUpdateShippingAddress()
        {
            var shipmentModels = new List<ShipmentViewModel>
            {
                new ShipmentViewModel {Address = new AddressModel()}
            };
            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel(),
                Shipments = shipmentModels
            };
            var updateModel = new UpdateAddressViewModel
            {
                BillingAddress = new AddressModel { AddressId = "BillingAddId" },
                Shipments = shipmentModels,
                ShippingAddressIndex = -1
            };

            _subject.ChangeAddress(viewModel, updateModel);

            Assert.Equal("BillingAddId", viewModel.BillingAddress.AddressId);
            Assert.NotEqual("ShipmentAddId", viewModel.Shipments[0].Address.AddressId);
        }

        private readonly CheckoutAddressHandling _subject;
        private readonly Mock<IAddressBookService> _addressBookServiceMock;

        public CheckoutAddressHandlingTests()
        {
            _addressBookServiceMock = new Mock<IAddressBookService>();
            _subject = new CheckoutAddressHandling(_addressBookServiceMock.Object);
        }
    }
}
