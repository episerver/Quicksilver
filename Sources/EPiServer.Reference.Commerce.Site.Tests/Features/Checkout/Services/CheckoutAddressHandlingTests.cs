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
        public void UpdateAuthenticatedUserAddresses_ShouldLoadAddressesFromAddressBook()
        {
            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel(),
                Shipments = new List<ShipmentViewModel>
                {
                    new ShipmentViewModel {Address = new AddressModel()}
                }
            };

            _subject.UpdateAuthenticatedUserAddresses(viewModel);

            _addressBookServiceMock.Verify(x => x.LoadAddress(viewModel.BillingAddress), Times.Once);
            _addressBookServiceMock.Verify(x => x.LoadAddress(viewModel.Shipments.Single().Address), Times.Once);
        }

        [Fact]
        public void UpdateAuthenticatedUserAddresses_WhenUseBillingAddressForShipmentIsFalse_ShouldNotSetShippingAddressAsBillingAddress()
        {
            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel(),
                Shipments = new List<ShipmentViewModel>
                {
                     new ShipmentViewModel {Address = new AddressModel()}
                }
            };

            _subject.UpdateAuthenticatedUserAddresses(viewModel);

            Assert.NotEqual(viewModel.BillingAddress, viewModel.Shipments.Single().Address);
        }

        [Fact]
        public void UpdateAnonymousUserAddresses_WhenUseBillingAddressForShipmentIsTrue_ShouldSetShippingAddressAsBillingAddress()
        {
            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel(),
                Shipments = new List<ShipmentViewModel>
                {
                     new ShipmentViewModel {Address = new AddressModel()}
                },
                UseBillingAddressForShipment = true
            };

            _subject.UpdateAnonymousUserAddresses(viewModel);

            Assert.Equal(viewModel.BillingAddress, viewModel.Shipments.Single().Address);
        }

        [Fact]
        public void UpdateAnonymousUserAddresses_WhenUseBillingAddressForShipmentIsFalse_ShouldNotSetShippingAddressAsBillingAddress()
        {
            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel(),
                Shipments = new List<ShipmentViewModel>
                {
                     new ShipmentViewModel {Address = new AddressModel()}
                }
            };

            _subject.UpdateAnonymousUserAddresses(viewModel);

            Assert.NotEqual(viewModel.BillingAddress, viewModel.Shipments.Single().Address);
        }

        [Fact]
        public void UpdateAuthenticatedUserAddresses_WhenUseBillingAddressForShipmentIsTrue_ShouldSetShippingAddressAsBillingAddress()
        {
            var viewModel = new CheckoutViewModel
            {
                BillingAddress = new AddressModel(),
                Shipments = new List<ShipmentViewModel>
                {
                     new ShipmentViewModel {Address = new AddressModel()}
                },
                UseBillingAddressForShipment = true
            };

            _subject.UpdateAuthenticatedUserAddresses(viewModel);

            Assert.Equal(viewModel.BillingAddress, viewModel.Shipments.Single().Address);
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
