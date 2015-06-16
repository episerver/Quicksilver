using System.Globalization;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.AddressBook;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Controllers;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Models;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Pages;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.AddressBook.Controllers
{
    [TestClass]
    public class AddressBookControllerTests
    {

        [TestMethod]
        public void Index_WhenCreatingViewModel_ShouldCallGetViewModelOnService()
        {
            var page = new AddressBookPage();

            _subject.Index(page);

            _addressBookServiceMock.Verify(s => s.GetViewModel(page));
        }

        [TestMethod]
        public void EditForm_WhenCalledWithPage_ShouldCallLoadFormModelOnService()
        {
            var page = new AddressBookPage();
            var guid = Guid.NewGuid();

            _subject.EditForm(page, guid);

            _addressBookServiceMock.Verify(s => s.LoadFormModel(It.IsAny<AddressBookFormModel>()));
        }

        [TestMethod]
        public void UpdateCountrySelection_WhenCalledWithFormModel_ShouldCallUpdateCountrySelectionOnService()
        {
            var formModel = new AddressBookFormModel();

            _subject.UpdateCountrySelection(formModel);

            _addressBookServiceMock.Verify(s => s.UpdateCountrySelection(formModel));
        }

        [TestMethod]
        public void Save_WhenModelStateIsValid_ShouldCallSaveOnService()
        {
            var formModel = new AddressBookFormModel();

            _subject.Save(formModel);

            _addressBookServiceMock.Verify(s => s.Save(formModel));
        }

        [TestMethod]
        public void Save_WhenModelStateIsNotValid_ShouldNotCallSaveOnService()
        {
            var formModel = new AddressBookFormModel();
            _subject.ModelState.AddModelError("test", "not valid");

            _subject.Save(formModel);

            _addressBookServiceMock.Verify(s => s.Save(formModel), Times.Never);
        }

        [TestMethod]
        public void Save_WhenAnotherAddressWithSameNameExists_ShouldNotSave()
        {
            var formModel = new AddressBookFormModel();
            _addressBookServiceMock.Setup(x => x.CanSave(It.IsAny<AddressBookFormModel>())).Returns(false);

            _subject.Save(formModel);

            _addressBookServiceMock.Verify(s => s.Save(formModel), Times.Never);
        }


        [TestMethod]
        public void Remove_ShouldCallDeleteOnService()
        {
            var guid = Guid.NewGuid();

            _subject.Remove(guid);

            _addressBookServiceMock.Verify(s => s.Delete(guid));
        }

        [TestMethod]
        public void SetPrimaryShippingAddress_ShouldCallSetPreferredShippingAddressOnService()
        {
            var guid = Guid.NewGuid();

            _subject.SetPreferredShippingAddress(guid);

            _addressBookServiceMock.Verify(s => s.SetPreferredShippingAddress(guid));
        }

        [TestMethod]
        public void SetPrimaryBillingAddress_ShouldCallSetPreferredBillingAddressOnService()
        {
            var guid = Guid.NewGuid();

            _subject.SetPreferredBillingAddress(guid);

            _addressBookServiceMock.Verify(s => s.SetPreferredBillingAddress(guid));
        }

        AddressBookController _subject;
        Mock<IContentLoader> _contentLoaderMock;
        Mock<IAddressBookService> _addressBookServiceMock;

        [TestInitialize]
        public void Setup()
        {
            _contentLoaderMock = new Mock<IContentLoader>();
            _addressBookServiceMock = new Mock<IAddressBookService>();
            _addressBookServiceMock.Setup(x => x.CanSave(It.IsAny<AddressBookFormModel>())).Returns(true);

            _contentLoaderMock.Setup(c => c.Get<StartPage>(ContentReference.StartPage)).Returns(new StartPage());

            var localizationService = new MemoryLocalizationService();
            localizationService.AddString(CultureInfo.CreateSpecificCulture("en"), "/AddressBook/Form/Error/ExistingAddress", "error");

            _subject = new AddressBookController(_contentLoaderMock.Object, _addressBookServiceMock.Object, localizationService);
        }
    }
}
