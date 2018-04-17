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
            var model = new AddressModel
            {
                AddressId = Guid.NewGuid().ToString(),
                Name = _address1.Name
            };

            Assert.False(_subject.CanSave(model));
        }

        [Fact]
        public void CanSave_WhenAddressWithSameNameDoesNotExist_ShouldReturnTrue()
        {
            var guid = Guid.NewGuid();
            var model = new AddressModel
            {
                AddressId = guid.ToString(),
                Name = guid.ToString()
            };

            Assert.True(_subject.CanSave(model));
        }

        [Fact]
        public void Save_WhenSavingNewAddress_ShouldAddAddressToCurrentContact()
        {
            var guid = Guid.NewGuid();
            var model = new AddressModel
            {
                Name = guid.ToString()
            };

            _subject.Save(model);

            Assert.NotNull(_currentContact.ContactAddresses.SingleOrDefault(x => x.AddressId == Guid.Parse(model.AddressId)));
        }

        [Fact]
        public void Save_WhenSavingExistingAddress_ShouldUpdateAddressInCurrentContact()
        {
            var model = new AddressModel
            {
                AddressId = ((Guid)_address1.AddressId).ToString(),
                Name = _address1.AddressId.ToString()
            };

            _subject.Save(model);

            Assert.NotNull(_currentContact.ContactAddresses.SingleOrDefault(x => x.Name == model.AddressId.ToString()));
        }

        [Fact]
        public void Save_WhenSavingNewAddress_ShouldUpdatePreferredBillingAddress()
        {
            var guid = Guid.NewGuid().ToString();
            var model = new AddressModel
            {
                AddressId = guid,
                Name = guid.ToString(),
                BillingDefault = true,
            };

            _subject.Save(model);

            Assert.Equal<Guid?>(Guid.Parse(model.AddressId), _currentContact.PreferredBillingAddress.AddressId);
        }

        [Fact]
        public void Save_WhenSavingNewAddress_ShouldUpdatePreferredShippingAddress()
        {
            var guid = Guid.NewGuid().ToString();
            var model = new AddressModel
            {
                AddressId = guid,
                Name = guid,
                ShippingDefault = true
            };

            _subject.Save(model);

            Assert.Equal<Guid?>(Guid.Parse(model.AddressId), _currentContact.PreferredShippingAddress.AddressId);
        }

        [Fact]
        public void Save_WhenSavingExistingAddress_ShouldUpdatePreferredBillingAddress()
        {
            var model = new AddressModel
            {
                AddressId = _address1.AddressId.ToString(),
                Name = _address1.AddressId.ToString(),
                BillingDefault = true,
            };

            _subject.Save(model);

            Assert.Equal<Guid?>(Guid.Parse(model.AddressId), _currentContact.PreferredBillingAddress.AddressId);
        }

        [Fact]
        public void Save_WhenSavingExistingAddress_ShouldUpdatePreferredShippingAddress()
        {
            var model = new AddressModel
            {
                AddressId = _address1.AddressId.ToString(),
                Name = _address1.AddressId.ToString(),
                ShippingDefault = true
            };

            _subject.Save(model);

            Assert.Equal<Guid?>(Guid.Parse(model.AddressId), _currentContact.PreferredShippingAddress.AddressId);
        }

        [InlineData(_address1Id)]
        [InlineData(_address2Id)]
        [Theory]
        public void List_ShouldReturnExistingAddresses(string addressId)
        {
            var id = new Guid(addressId);
            var result = _subject.List();

            Assert.Equal<int>(2, result.Count());
            Assert.NotNull(result.Single(x => x.AddressId == id.ToString()));
        }

        [Fact]
        public void SetPreferredBillingAddress_WhenAddressIsInvalid_ShouldNotUpdatePreferredBillingAddress()
        {
            _currentContact.PreferredBillingAddress = null;

            _subject.SetPreferredBillingAddress(Guid.Empty.ToString());

            Assert.Null(_currentContact.PreferredBillingAddress);
        }

        [Fact]
        public void SetPreferredBillingAddress_WhenAddressExists_ShouldUpdatePreferredBillingAddress()
        {
            _subject.SetPreferredBillingAddress(_address1.AddressId.ToString());

            Assert.Equal<Guid?>(_address1.AddressId, _currentContact.PreferredBillingAddress.AddressId);
        }

        [Fact]
        public void SetPreferredShippingAddress_WhenAddressIsInvalid_ShouldNotUpdatePreferredShippingAddress()
        {
            _currentContact.PreferredShippingAddress = null;

            _subject.SetPreferredShippingAddress(Guid.Empty.ToString());

            Assert.Null(_currentContact.PreferredShippingAddress);
        }

        [Fact]
        public void SetPreferredShippingAddress_WhenAddressExists_ShouldUpdatePreferredShippingAddress()
        {
            _subject.SetPreferredShippingAddress(_address1.AddressId.ToString());

            Assert.Equal<Guid?>(_address1.AddressId, _currentContact.PreferredShippingAddress.AddressId);
        }

        [Fact]
        public void Delete_WhenAddressDoesNotExist_ShouldNotDeleteAddress()
        {
            _subject.Delete(Guid.Empty.ToString());

            Assert.Equal<int>(2, _currentContact.ContactAddresses.Count());
        }

        [Fact]
        public void Delete_WhenAddressExists_ShouldDeleteAddress()
        {
            _subject.Delete(_address1.AddressId.ToString());

            Assert.Single(_currentContact.ContactAddresses);
        }

        [Fact]
        public void Delete_WhenAddressExistsAndIsSetAsPreferredBillingAddress_ShouldDeleteAddressAndUpdatePreferredBillingAddress()
        {
            _subject.SetPreferredBillingAddress(_address1.AddressId.ToString());

            _subject.Delete(_address1.AddressId.ToString());

            Assert.Null(_currentContact.PreferredBillingAddressId);
        }

        [Fact]
        public void Delete_WhenAddressExistsAndIsSetAsPreferredShippingAddress_ShouldDeleteAddressAndUpdatePreferredShippingAddress()
        {
            _subject.SetPreferredShippingAddress(_address1.AddressId.ToString());

            _subject.Delete(_address1.AddressId.ToString());

            Assert.Null(_currentContact.PreferredShippingAddressId);
        }

        [Fact]
        public void GetAddressBookViewModel_WhenPassingPage_ShouldReturnModel()
        {
            var page = new AddressBookPage();
            var result = _subject.GetAddressBookViewModel(page);

            Assert.Equal<AddressBookPage>(page, result.CurrentPage);
        }

        [Fact]
        public void LoadAddress_WhenModelHasNoAddressId_ShouldReturnEmptyModel()
        {
            var model = new AddressModel();
            _subject.LoadAddress(model);

            Assert.Null(model.AddressId);
        }

        [Fact]
        public void LoadAddress_WhenModelHasNoCountryCode_ShouldReturnModelWithoutCountryCode()
        {
            var model = new AddressModel();
            _subject.LoadAddress(model);

            Assert.Null(model.CountryCode);
        }

        [Fact]
        public void ConvertToModel_WhenOrderAddressIsNull_ShouldNotSetAddressIdOnModel()
        {
            var model = _subject.ConvertToModel(null);
            Assert.Null(model.AddressId);
        }

        [Fact]
        public void ConvertToModel_WhenOrderAddressIsNotNull_ShouldReturnModelWithSameValueAsOrderAddress()
        {
            var orderAddress = new FakeOrderAddress()
            {
                Id = "SampleId",
                FirstName = "first",
                LastName = "last"
            };
            var model = _subject.ConvertToModel(orderAddress);
            Assert.Equal(model.AddressId, orderAddress.Id);
            Assert.Equal(model.FirstName, orderAddress.FirstName);
            Assert.Equal(model.LastName, orderAddress.LastName);
        }

        private readonly AddressBookService _subject;
        private readonly CustomerAddress _address1;
        private const string _address1Id = "6BB9294A-F25F-4145-A225-F8F4D675377B";
        private const string _address2Id = "B374E959-B221-4B3C-ACC6-CE0B837C5765";
        private readonly FakeCurrentContact _currentContact;

        public AddressBookServiceTests()
        {
            _address1 = CustomerAddress.CreateInstance();
            _address1.AddressId = new PrimaryKeyId(new Guid(_address1Id));
            _address1.Name = _address1.AddressId.ToString();

            var address2 = CustomerAddress.CreateInstance();
            address2.AddressId = new PrimaryKeyId(new Guid(_address2Id));
            address2.Name = address2.AddressId.ToString();            

            _currentContact = new FakeCurrentContact(new[] { _address1, address2 })
            {
                PreferredBillingAddress = _address1,
                PreferredShippingAddress = _address1
            };
            var customerContext = new FakeCustomerContext(_currentContact);
            var countryManager = new FakeCountryManager();
            var fakeOrderGroupFactory = new FakeOrderGroupFactory();

            _subject = new AddressBookService(customerContext, countryManager, fakeOrderGroupFactory);
        }
    }
}
