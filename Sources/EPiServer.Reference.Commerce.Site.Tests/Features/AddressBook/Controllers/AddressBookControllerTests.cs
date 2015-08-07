using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Controllers;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Pages;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Web.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;

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

            _addressBookServiceMock.Verify(s => s.GetAddressBookViewModel(page));
        }

        [TestMethod]
        public void EditForm_WhenCalledWithPage_ShouldCallLoadAddressOnService()
        {
            var page = new AddressBookPage();
            var guid = Guid.NewGuid();

            _subject.EditForm(page, guid);

            _addressBookServiceMock.Verify(s => s.LoadAddress(It.IsAny<Address>()));
        }

        [TestMethod]
        public void Save_WhenModelStateIsValid_ShouldCallSaveOnService()
        {
            var viewModel = new AddressViewModel { Address = new Address() };
            AddressBookPage currentPage = new AddressBookPage();
            viewModel.Address.Name = "name";

            _subject.Save(currentPage, viewModel);

            _addressBookServiceMock.Verify(s => s.Save(viewModel.Address));
        }

        [TestMethod]
        public void Save_WhenModelStateIsNotValid_ShouldNotCallSaveOnService()
        {
            AddressBookPage currentPage = new AddressBookPage();
            var viewModel = new AddressViewModel { Address = new Address() };
            _subject.ModelState.AddModelError("test", "not valid");

            _subject.Save(currentPage, viewModel);

            _addressBookServiceMock.Verify(s => s.Save(viewModel.Address), Times.Never);
        }

        [TestMethod]
        public void Save_WhenAnotherAddressWithSameNameExists_ShouldNotSave()
        {
            AddressBookPage currentPage = new AddressBookPage();
            var viewModel = new AddressViewModel{ Address = new Address() };
            _addressBookServiceMock.Setup(x => x.CanSave(It.IsAny<Address>())).Returns(false);

            _subject.Save(currentPage, viewModel);

            _addressBookServiceMock.Verify(s => s.Save(viewModel.Address), Times.Never);
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

        [TestMethod]
        public void OnException_ShouldDelegateToExceptionHandler()
        {
            var testController = CreateTestController();
            testController.CallOnException(_exceptionContext);

            _controllerExceptionHandler.Verify(x => x.HandleRequestValidationException(_exceptionContext, "save", testController.OnSaveException));
        }

        [TestMethod]
        public void OnSaveException_WhenAddressIdDontExist_ShouldCreateViewModelWithoutAddressIdSet()
        {
            //Setup
            {
                Setup_form_on_HttpRequestBase(new NameValueCollection());
                Setup_exception(new HttpRequestValidationException());
                Setup_RequestContext_to_contain_routed_data(new AddressBookPage());
                Setup_GetAddressBookViewModel_to_return_model_having_same_page_as_inparameter();
            }

            var result = _subject.OnSaveException(_exceptionContext);

            Assert.IsInstanceOfType(((ViewResult)result).Model, typeof(AddressViewModel));
        }

        [TestMethod]
        public void OnSaveException_WhenAddressIdDontExist_ShouldReturnActionResult()
        {
            //Setup
            {
                Setup_form_on_HttpRequestBase(new NameValueCollection());
                Setup_exception(new HttpRequestValidationException());
                Setup_RequestContext_to_contain_routed_data(new AddressBookPage());
                Setup_GetAddressBookViewModel_to_return_model_having_same_page_as_inparameter();
            }

            var result = _subject.OnSaveException(_exceptionContext);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void OnSaveException_WhenAddressIdExist_ShouldCreateViewModelWithAddressIdSet()
        {
            var guid = Guid.NewGuid();

            //Setup
            {
                Setup_form_on_HttpRequestBase(new NameValueCollection() { { "addressId", guid.ToString() } });
                Setup_exception(new HttpRequestValidationException());
                Setup_RequestContext_to_contain_routed_data(new AddressBookPage());
                Setup_GetAddressBookViewModel_to_return_model_having_same_page_as_inparameter();
            }

            var result = _subject.OnSaveException(_exceptionContext);

            var model = ((ViewResult)result).Model as AddressViewModel;
            Assert.AreEqual<Guid?>(guid, model.Address.AddressId);
        }

        [TestMethod]
        public void OnSaveException_WhenCurrentPageDontExist_ShouldNotReturnResult()
        {
            var guid = Guid.NewGuid();

            //Setup
            {
                Setup_form_on_HttpRequestBase(new NameValueCollection() { { "addressId", guid.ToString() } });
                Setup_exception(new HttpRequestValidationException());
                Setup_RequestContext_to_contain_routed_data(null);
                Setup_GetAddressBookViewModel_to_return_model_having_same_page_as_inparameter();
            }

            var result = _subject.OnSaveException(_exceptionContext);

            Assert.IsInstanceOfType(result, typeof(EmptyResult));
        }

        [TestMethod]
        public void OnSaveException_WhenCurrentPageDontExist_ShouldReturnEmptyResult()
        {
            var guid = Guid.NewGuid();

            //Setup
            {
                Setup_form_on_HttpRequestBase(new NameValueCollection() { { "addressId", guid.ToString() } });
                Setup_exception(new HttpRequestValidationException());
                Setup_RequestContext_to_contain_routed_data(null);
                Setup_GetAddressBookViewModel_to_return_model_having_same_page_as_inparameter();
            }

            var result = _subject.OnSaveException(_exceptionContext);

            Assert.IsInstanceOfType(result, typeof(EmptyResult));
        }

        [TestMethod]
        public void OnSaveException_WhenCurrentPageExist_ShouldCreateViewModelWithCurrentPageSet()
        {
            var guid = Guid.NewGuid();
            var currentPage = new AddressBookPage();

            //Setup
            {
                Setup_form_on_HttpRequestBase(new NameValueCollection() { { "addressId", guid.ToString() } });
                Setup_exception(new HttpRequestValidationException());
                Setup_RequestContext_to_contain_routed_data(currentPage);
                Setup_GetAddressBookViewModel_to_return_model_having_same_page_as_inparameter();
            }

            var result = _subject.OnSaveException(_exceptionContext);

            var model = ((ViewResult)result).Model as AddressViewModel;
            Assert.AreEqual<AddressBookPage>(currentPage, model.CurrentPage);

            Assert.IsInstanceOfType(_exceptionContext.Result, typeof(EmptyResult));
        }

        [TestMethod]
        public void OnSaveException_WhenActionIsSave_ShouldCreateAddressBookFormModel()
        {
            var guid = Guid.NewGuid();
            var currentPage = new AddressBookPage();

            //Setup
            {
                Setup_form_on_HttpRequestBase(new NameValueCollection() { { "addressId", guid.ToString() } });
                Setup_exception(new HttpRequestValidationException());
                Setup_RequestContext_to_contain_routed_data(currentPage);
                Setup_GetAddressBookViewModel_to_return_model_having_same_page_as_inparameter();

                Setup_exception(new HttpRequestValidationException());
                Setup_action_for_controller("save", _subject);
            }

            var result = _subject.OnSaveException(_exceptionContext);

            Assert.IsInstanceOfType(((ViewResult)result).Model, typeof(AddressViewModel));
        }

        AddressBookController _subject;
        MemoryLocalizationService _localizationService;
        Mock<IContentLoader> _contentLoaderMock;
        Mock<IAddressBookService> _addressBookServiceMock;
        Mock<HttpRequestBase> _httpRequestBase;
        Mock<HttpContextBase> _httpContextBase;
        Mock<RequestContext> _requestContext;
        Mock<ControllerExceptionHandler> _controllerExceptionHandler;
        ExceptionContext _exceptionContext;

        [TestInitialize]
        public void Setup()
        {
            _controllerExceptionHandler = new Mock<ControllerExceptionHandler>();
            _requestContext = new Mock<RequestContext>();
            _httpRequestBase = new Mock<HttpRequestBase>();

            _httpContextBase = new Mock<HttpContextBase>();
            _httpContextBase.Setup(x => x.Request).Returns(_httpRequestBase.Object);

            _exceptionContext = new ExceptionContext
            {
                HttpContext = _httpContextBase.Object,
                RequestContext = _requestContext.Object
            };

            _contentLoaderMock = new Mock<IContentLoader>();
            _addressBookServiceMock = new Mock<IAddressBookService>();
            _addressBookServiceMock.Setup(x => x.CanSave(It.IsAny<Address>())).Returns(true);

            _contentLoaderMock.Setup(c => c.Get<StartPage>(ContentReference.StartPage)).Returns(new StartPage());

            _localizationService = new MemoryLocalizationService();
            _localizationService.AddString(CultureInfo.CreateSpecificCulture("en"), "/AddressBook/Form/Error/ExistingAddress", "error");

            _subject = new AddressBookController(_contentLoaderMock.Object, _addressBookServiceMock.Object, _localizationService, _controllerExceptionHandler.Object);
        }

        private AddressBookControllerForTest CreateTestController()
        {
            return new AddressBookControllerForTest(_contentLoaderMock.Object, _addressBookServiceMock.Object, _localizationService, _controllerExceptionHandler.Object);
        }

        private void Setup_action_for_controller(string actionName, Controller controller)
        {
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.RouteData.Values.Add("action", actionName);
        }

        private void Setup_RequestContext_to_contain_routed_data(object rotedData)
        {
            var routeData = new RouteData();
            routeData.DataTokens.Add(RoutingConstants.RoutedDataKey, rotedData);

            _requestContext.Setup(x => x.RouteData).Returns(routeData);
        }

        private void Setup_exception(Exception exception)
        {
            _exceptionContext.Exception = exception;
        }

        private void Setup_form_on_HttpRequestBase(NameValueCollection nameValueCollection)
        {
            _httpRequestBase.Setup(x => x.Form).Returns(nameValueCollection);
        }

        private void Setup_GetAddressBookViewModel_to_return_model_having_same_page_as_inparameter()
        {
            _addressBookServiceMock.Setup(x => x.GetAddressBookViewModel(It.IsAny<AddressBookPage>())).Returns((AddressBookPage page) => new AddressCollectionViewModel { CurrentPage = page });
        }

        private class AddressBookControllerForTest : AddressBookController
        {
            public AddressBookControllerForTest(IContentLoader contentLoader, IAddressBookService addressBookService, LocalizationService localizationService, ControllerExceptionHandler controllerExceptionHandler)
                : base(contentLoader, addressBookService, localizationService, controllerExceptionHandler)
            {
            }

            public void CallOnException(ExceptionContext filterContext)
            {
                OnException(filterContext);
            }
        }
    }
}

