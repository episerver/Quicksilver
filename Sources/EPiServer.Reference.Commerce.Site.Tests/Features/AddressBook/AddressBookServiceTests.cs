using EPiServer.Reference.Commerce.Site.Features.AddressBook.Pages;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes;
using Mediachase.BusinessFoundation.Data;
using Mediachase.Commerce.Customers;
using System;
using System.Linq;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.AddressBook
{
    public class AddressBookServiceTests
    {
        [Fact]
        public void CanSave_WhenAddressWithSameNameExists_ShouldReturnFalse()
        {
            var model = new Address
            {
                AddressId = Guid.NewGuid(),
                Name = _address1.Name
            };

            Assert.False(_subject.CanSave(model));
        }

        [Fact]
        public void CanSave_WhenAddressWithSameNameDoesNotExist_ShouldReturnTrue()
        {
            var guid = Guid.NewGuid();
            var model = new Address
            {
                AddressId = guid,
                Name = guid.ToString()
            };

            Assert.True(_subject.CanSave(model));
        }

        [Fact]
        public void Save_WhenSavingNewAddress_ShouldAddAddressToCurrentContact()
        {
            var guid = Guid.NewGuid();
            var model = new Address
            {
                Name = guid.ToString()
            };

            _subject.Save(model);

            Assert.NotNull(_currentContact.ContactAddresses.SingleOrDefault(x => x.AddressId == model.AddressId));
        }

        [Fact]
        public void Save_WhenSavingExistingAddress_ShouldUpdateAddressInCurrentContact()
        {
            var model = new Address
            {
                AddressId = _address1.AddressId,
                Name = _address1.AddressId.ToString()
            };

            _subject.Save(model);

            Assert.NotNull(_currentContact.ContactAddresses.SingleOrDefault(x => x.Name == model.AddressId.ToString()));
        }

        [Fact]
        public void Save_WhenSavingNewAddress_ShouldUpdatePreferredBillingAddress()
        {
            var guid = Guid.NewGuid();
            var model = new Address
            {
                AddressId = guid,
                Name = guid.ToString(),
                BillingDefault = true,
            };

            _subject.Save(model);

            Assert.Equal<Guid?>(model.AddressId, _currentContact.PreferredBillingAddress.AddressId);
        }

        [Fact]
        public void Save_WhenSavingNewAddress_ShouldUpdatePreferredShippingAddress()
        {
            var guid = Guid.NewGuid();
            var model = new Address
            {
                AddressId = guid,
                Name = guid.ToString(),
                ShippingDefault = true
            };

            _subject.Save(model);

            Assert.Equal<Guid?>(model.AddressId, _currentContact.PreferredShippingAddress.AddressId);
        }

        [Fact]
        public void Save_WhenSavingExistingAddress_ShouldUpdatePreferredBillingAddress()
        {
            var model = new Address
            {
                AddressId = _address1.AddressId,
                Name = _address1.AddressId.ToString(),
                BillingDefault = true,
            };

            _subject.Save(model);

            Assert.Equal<Guid?>(model.AddressId, _currentContact.PreferredBillingAddress.AddressId);
        }

        [Fact]
        public void Save_WhenSavingExistingAddress_ShouldUpdatePreferredShippingAddress()
        {
            var model = new Address
            {
                AddressId = _address1.AddressId,
                Name = _address1.AddressId.ToString(),
                ShippingDefault = true
            };

            _subject.Save(model);

            Assert.Equal<Guid?>(model.AddressId, _currentContact.PreferredShippingAddress.AddressId);
        }

        [InlineData(_address1Id)]
        [InlineData(_address2Id)]
        public void GetAvailableShippingAddresses_ShouldReturnExistingAddresses(string addressId)
        {
            var id = new Guid(addressId);
            var result = _subject.GetAvailableShippingAddresses();

            Assert.Equal<int>(2, result.Count());
            Assert.NotNull(result.Single(x => x.AddressId == id));
        }

        [Fact]
        public void SetPrimaryBillingAddress_WhenAddressIsInvalid_ShouldNotUpdatePreferredBillingAddress()
        {
            _subject.SetPreferredBillingAddress(Guid.Empty);

            Assert.Null(_currentContact.PreferredBillingAddress);
        }

        [Fact]
        public void SetPrimaryBillingAddress_WhenAddressExists_ShouldUpdatePreferredBillingAddress()
        {
            _subject.SetPreferredBillingAddress(_address1.AddressId);

            Assert.Equal<Guid?>(_address1.AddressId, _currentContact.PreferredBillingAddress.AddressId);
        }

        [Fact]
        public void SetPrimaryShippingAddress_WhenAddressIsInvalid_ShouldNotUpdatePreferredShippingAddress()
        {
            _subject.SetPreferredShippingAddress(Guid.Empty);

            Assert.Null(_currentContact.PreferredShippingAddress);
        }

        [Fact]
        public void SetPrimaryShippingAddress_WhenAddressExists_ShouldUpdatePreferredShippingAddress()
        {
            _subject.SetPreferredShippingAddress(_address1.AddressId);

            Assert.Equal<Guid?>(_address1.AddressId, _currentContact.PreferredShippingAddress.AddressId);
        }

        [Fact]
        public void Delete_WhenAddressDoesNotExist_ShouldNotDeleteAddress()
        {
            _subject.Delete(Guid.Empty);

            Assert.Equal<int>(2, _currentContact.ContactAddresses.Count());
        }

        [Fact]
        public void Delete_WhenAddressExists_ShouldDeleteAddress()
        {
            _subject.Delete(_address1.AddressId);

            Assert.Equal<int>(1, _currentContact.ContactAddresses.Count());
        }

        [Fact]
        public void Delete_WhenAddressExistsAndIsSetAsPreferredBillingAddress_ShouldDeleteAddressAndUpdatePreferredBillingAddress()
        {
            _subject.SetPreferredBillingAddress(_address1.AddressId);

            _subject.Delete(_address1.AddressId);

            Assert.Null(_currentContact.PreferredBillingAddressId);
        }

        [Fact]
        public void Delete_WhenAddressExistsAndIsSetAsPreferredShippingAddress_ShouldDeleteAddressAndUpdatePreferredShippingAddress()
        {
            _subject.SetPreferredShippingAddress(_address1.AddressId);

            _subject.Delete(_address1.AddressId);

            Assert.Null(_currentContact.PreferredShippingAddressId);
        }

        [Fact]
        public void GetViewModel_WhenPassingPage_ShouldReturnModel()
        {
            var page = new AddressBookPage();
            var result = _subject.GetAddressBookViewModel(page);

            Assert.Equal<AddressBookPage>(page, result.CurrentPage);
        }

        [Fact]
        public void LoadAddress_WhenModelHasNoAddressId_ShouldReturnEmptyModel()
        {
            var model = new Address();
            _subject.LoadAddress(model);

            Assert.Null(model.AddressId);
        }

        private AddressBookService _subject;
        private CustomerAddress _address1;
        private const string _address1Id = "6BB9294A-F25F-4145-A225-F8F4D675377B";
        private CustomerAddress _address2;
        private const string _address2Id = "B374E959-B221-4B3C-ACC6-CE0B837C5765";
        private FakeCurrentContact _currentContact;

        public AddressBookServiceTests()
        {
            _address1 = CustomerAddress.CreateInstance();
            _address1.AddressId = new PrimaryKeyId(new Guid(_address1Id));
            _address1.Name = _address1.AddressId.ToString();

            _address2 = CustomerAddress.CreateInstance();
            _address2.AddressId = new PrimaryKeyId(new Guid(_address2Id));
            _address2.Name = _address2.AddressId.ToString();

            _currentContact = new FakeCurrentContact(new[] { _address1, _address2 });
            var customerContext = new FakeCustomerContext(_currentContact);
            var countryManager = new FakeCountryManager();

            _subject = new AddressBookService(customerContext, countryManager);
        }
    }
}
