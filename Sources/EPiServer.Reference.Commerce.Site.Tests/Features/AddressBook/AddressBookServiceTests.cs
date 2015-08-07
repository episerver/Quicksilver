using System;
using System.Linq;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes;
using Mediachase.BusinessFoundation.Data;
using Mediachase.Commerce.Customers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Pages;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.AddressBook
{
    [TestClass]
    public class AddressBookServiceTests
    {
        [TestMethod]
        public void CanSave_WhenAddressWithSameNameExists_ShouldReturnFalse()
        {
            var model = new Address
            {
                AddressId = Guid.NewGuid(),
                Name = _address1.Name
            };
            
            Assert.IsFalse(_subject.CanSave(model));
        }

        [TestMethod]
        public void CanSave_WhenAddressWithSameNameDoesNotExist_ShouldReturnTrue()
        {
            var guid = Guid.NewGuid();
            var model = new Address
            {
                AddressId = guid,
                Name = guid.ToString()
            };

            Assert.IsTrue(_subject.CanSave(model));
        }

        [TestMethod]
        public void Save_WhenSavingNewAddress_ShouldAddAddressToCurrentContact()
        {
            var guid = Guid.NewGuid();
            var model = new Address
            {
                Name = guid.ToString()
            };

            _subject.Save(model);
           
            Assert.IsNotNull(_currentContact.ContactAddresses.SingleOrDefault(x => x.AddressId == model.AddressId));
        }

        [TestMethod]
        public void Save_WhenSavingExistingAddress_ShouldUpdateAddressInCurrentContact()
        {
            var model = new Address
            {
                AddressId = _address1.AddressId,
                Name = _address1.AddressId.ToString()
            };

            _subject.Save(model);
            
            Assert.IsNotNull(_currentContact.ContactAddresses.SingleOrDefault(x => x.Name == model.AddressId.ToString()));
        }

        [TestMethod]
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

            Assert.AreEqual<Guid?>(model.AddressId, _currentContact.PreferredBillingAddress.AddressId);
        }

        [TestMethod]
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

            Assert.AreEqual<Guid?>(model.AddressId, _currentContact.PreferredShippingAddress.AddressId);
        }

        [TestMethod]
        public void Save_WhenSavingExistingAddress_ShouldUpdatePreferredBillingAddress()
        {
            var model = new Address
            {
                AddressId = _address1.AddressId,
                Name = _address1.AddressId.ToString(),
                BillingDefault = true,
            };

            _subject.Save(model);

            Assert.AreEqual<Guid?>(model.AddressId, _currentContact.PreferredBillingAddress.AddressId);
        }

        [TestMethod]
        public void Save_WhenSavingExistingAddress_ShouldUpdatePreferredShippingAddress()
        {
            var model = new Address
            {
                AddressId = _address1.AddressId,
                Name = _address1.AddressId.ToString(),
                ShippingDefault = true
            };

            _subject.Save(model);

            Assert.AreEqual<Guid?>(model.AddressId, _currentContact.PreferredShippingAddress.AddressId);
        }

        [TestMethod]
        public void SetPrimaryBillingAddress_WhenAddressIsInvalid_ShouldNotUpdatePreferredBillingAddress()
        {
            _subject.SetPreferredBillingAddress(Guid.Empty);

            Assert.IsNull(_currentContact.PreferredBillingAddress);
        }

        [TestMethod]
        public void SetPrimaryBillingAddress_WhenAddressExists_ShouldUpdatePreferredBillingAddress()
        {
            _subject.SetPreferredBillingAddress(_address1.AddressId);

            Assert.AreEqual<Guid?>(_address1.AddressId, _currentContact.PreferredBillingAddress.AddressId);
        }

        [TestMethod]
        public void SetPrimaryShippingAddress_WhenAddressIsInvalid_ShouldNotUpdatePreferredShippingAddress()
        {
            _subject.SetPreferredShippingAddress(Guid.Empty);

            Assert.IsNull(_currentContact.PreferredShippingAddress);
        }

        [TestMethod]
        public void SetPrimaryShippingAddress_WhenAddressExists_ShouldUpdatePreferredShippingAddress()
        {
            _subject.SetPreferredShippingAddress(_address1.AddressId);

            Assert.AreEqual<Guid?>(_address1.AddressId, _currentContact.PreferredShippingAddress.AddressId);
        }

        [TestMethod]
        public void Delete_WhenAddressDoesNotExist_ShouldNotDeleteAddress()
        {
            _subject.Delete(Guid.Empty);

            Assert.AreEqual<int>(2, _currentContact.ContactAddresses.Count());
        }

        [TestMethod]
        public void Delete_WhenAddressExists_ShouldDeleteAddress()
        {
            _subject.Delete(_address1.AddressId);

            Assert.AreEqual<int>(1, _currentContact.ContactAddresses.Count());
        }

        [TestMethod]
        public void Delete_WhenAddressExistsAndIsSetAsPreferredBillingAddress_ShouldDeleteAddressAndUpdatePreferredBillingAddress()
        {
            _subject.SetPreferredBillingAddress(_address1.AddressId);
            
            _subject.Delete(_address1.AddressId);

            Assert.IsNull(_currentContact.PreferredBillingAddressId);
        }

        [TestMethod]
        public void Delete_WhenAddressExistsAndIsSetAsPreferredShippingAddress_ShouldDeleteAddressAndUpdatePreferredShippingAddress()
        {
            _subject.SetPreferredShippingAddress(_address1.AddressId);

            _subject.Delete(_address1.AddressId);

            Assert.IsNull(_currentContact.PreferredShippingAddressId);
        }

        [TestMethod]
        public void GetViewModel_WhenPassingPage_ShouldReturnModel()
        {
            var page = new AddressBookPage();
            var result = _subject.GetAddressBookViewModel(page);

            Assert.AreEqual<AddressBookPage>(page, result.CurrentPage);
        }

        [TestMethod]
        public void LoadAddress_WhenModelHasNoAddressId_ShouldReturnEmptyModel()
        {
            var model = new Address();
            _subject.LoadAddress(model);

            Assert.IsNull(model.AddressId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LoadAddress_WhenAddressIdDoesNotExist_ShouldThrowException()
        {
            _currentContact = new FakeCurrentContact(Enumerable.Empty<CustomerAddress>());
            var customerContext = new FakeCustomerContext(_currentContact);
            var countryManager = new FakeCountryManager();
            _subject = new AddressBookService(customerContext, countryManager);

            var model = new Address();
            model.AddressId = Guid.NewGuid();
            _subject.LoadAddress(model);
        }

        private AddressBookService _subject;
        private CustomerAddress _address1;
        private CustomerAddress _address2;
        private FakeCurrentContact _currentContact;

        [TestInitialize]
        public void Setup()
        {
            _address1 = CustomerAddress.CreateInstance();
            _address1.AddressId = new PrimaryKeyId(Guid.NewGuid());
            _address1.Name = _address1.AddressId.ToString();

            _address2 = CustomerAddress.CreateInstance();
            _address2.AddressId = new PrimaryKeyId(Guid.NewGuid());
            _address2.Name = _address2.AddressId.ToString();
            
            _currentContact = new FakeCurrentContact(new[] { _address1, _address2 });
            var customerContext = new FakeCustomerContext(_currentContact);
            var countryManager = new FakeCountryManager();

            _subject = new AddressBookService(customerContext, countryManager);
        }
    }
}
