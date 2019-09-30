using System;
using System.Linq;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Services
{
    public class CheckoutAddressHandling
    {
        private readonly IAddressBookService _addressBookService;

        public CheckoutAddressHandling(IAddressBookService addressBookService)
        {
            _addressBookService = addressBookService;
        }

        public virtual void UpdateUserAddresses(CheckoutViewModel viewModel)
        {
            if (viewModel.IsAuthenticated)
            {
                UpdateAuthenticatedUserAddresses(viewModel);
            }
            else
            {
                UpdateAnonymousUserAddresses(viewModel);
            }
        }

        public virtual void ChangeAddress(CheckoutViewModel viewModel, UpdateAddressViewModel updateViewModel)
        {
            var isShippingAddressUpdated = updateViewModel.ShippingAddressIndex > -1;

            var updatedAddress = isShippingAddressUpdated ? 
                updateViewModel.Shipments[updateViewModel.ShippingAddressIndex].Address : 
                updateViewModel.BillingAddress;

            if (updatedAddress.AddressId != null)
            {
                _addressBookService.LoadAddress(updatedAddress);
            }

            _addressBookService.LoadCountriesAndRegionsForAddress(updatedAddress);

            viewModel.UseBillingAddressForShipment = updateViewModel.UseBillingAddressForShipment;
            viewModel.BillingAddress = updateViewModel.BillingAddress;

            if (isShippingAddressUpdated)
            {
                _addressBookService.LoadAddress(viewModel.BillingAddress);
                _addressBookService.LoadCountriesAndRegionsForAddress(viewModel.BillingAddress);
                _addressBookService.LoadAddress(updatedAddress);
                viewModel.Shipments[updateViewModel.ShippingAddressIndex].Address = updatedAddress;
            }

            foreach (var shipment in viewModel.Shipments)
            {
                _addressBookService.LoadCountriesAndRegionsForAddress(shipment.Address);
            }
        }

        private void SetDefaultBillingAddressName(CheckoutViewModel viewModel)
        {
            Guid guid;
            if (Guid.TryParse(viewModel.BillingAddress.Name, out guid))
            {
                viewModel.BillingAddress.Name = "Billing address (" + viewModel.BillingAddress.Line1 + ")";
            }
        }

        private void SetDefaultShippingAddressesNames(CheckoutViewModel viewModel)
        {
            foreach (var address in viewModel.Shipments.Select(x => x.Address))
            {
                Guid guid;
                if (Guid.TryParse(address.Name, out guid))
                {
                    address.Name = "Shipping address (" + address.Line1 + ")";
                }
            }
        }

        private void LoadBillingAddressFromAddressBook(CheckoutViewModel checkoutViewModel)
        {
            _addressBookService.LoadAddress(checkoutViewModel.BillingAddress);
        }

        private void LoadShippingAddressesFromAddressBook(CheckoutViewModel checkoutViewModel)
        {
            foreach (var shipment in checkoutViewModel.Shipments)
            {
                _addressBookService.LoadAddress(shipment.Address);
            }
        }

        private void UpdateAuthenticatedUserAddresses(CheckoutViewModel viewModel)
        {
            LoadBillingAddressFromAddressBook(viewModel);
            LoadShippingAddressesFromAddressBook(viewModel);
            if (viewModel.UseBillingAddressForShipment || viewModel.Shipments.Any(s => string.IsNullOrEmpty(s.Address?.Name)))
            {
                foreach (var item in viewModel.Shipments)
                {
                    item.Address = viewModel.BillingAddress;
                }
            }
        }

        private void UpdateAnonymousUserAddresses(CheckoutViewModel viewModel)
        {
            SetDefaultBillingAddressName(viewModel);

            if (viewModel.UseBillingAddressForShipment || viewModel.Shipments.Any(s => string.IsNullOrEmpty(s.Address?.Name)))
            {
                SetDefaultShippingAddressesNames(viewModel);
                foreach (var item in viewModel.Shipments)
                {
                    item.Address = viewModel.BillingAddress;
                }
                
            }
        }
    }
}