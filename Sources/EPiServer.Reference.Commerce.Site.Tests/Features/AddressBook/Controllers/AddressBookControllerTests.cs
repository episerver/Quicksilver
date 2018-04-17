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
using Moq;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.AddressBook.Controllers
{

    public class AddressBookControllerTests
    {

        [Fact]
        public void Index_WhenCreatingViewModel_ShouldCallGetViewModelOnService()
        {
            var page = new AddressBookPage();

            _subject.Index(page);

            _addressBookServiceMock.Verify(s => s.GetAddressBookViewModel(page));
        }

        [Fact]
        public void EditForm_WhenCalledWithPage_ShouldCallLoadAddressOnService()
        {
            var page = new AddressBookPage();
            var guid = Guid.NewGuid();

            _subject.EditForm(page, guid.ToString());

            _addressBookServiceMock.Verify(s => s.LoadAddress(It.IsAny<AddressModel>()));
        }

        [Fact]
        public void Save_WhenModelStateIsValid_ShouldCallSaveOnService()
        {
            var viewModel = new AddressViewModel
            {
                CurrentPage = new AddressBookPage(),
                Address = new AddressModel
                {
                    Name = "name"
                }
            };

            _subject.Save(viewModel);

            _addressBookServiceMock.Verify(s => s.Save(viewModel.Address));
        }

        [Fact]
        public void Save_WhenModelStateIsNotValid_ShouldNotCallSaveOnService()
        {
            var viewModel = new AddressViewModel
            {
                CurrentPage = new AddressBookPage(),
                Address = new AddressModel()
            };

            _subject.ModelState.AddModelError("test", "not valid");

            _subject.Save(viewModel);

            _addressBookServiceMock.Verify(s => s.Save(viewModel.Address), Times.Never);
        }

        [Fact]
        public void Save_WhenAnotherAddressWithSameNameExists_ShouldNotSave()
        {
            var viewModel = new AddressViewModel
            {
                CurrentPage = new AddressBookPage(),
                Address = new AddressModel()
            };

            _addressBookServiceMock.Setup(x => x.CanSave(It.IsAny<AddressModel>())).Returns(false);

            _subject.Save(viewModel);

            _addressBookServiceMock.Verify(s => s.Save(viewModel.Address), Times.Never);
        }


        [Fact]
        public void Remove_ShouldCallDeleteOnService()
        {
            var guid = Guid.NewGuid();

            _subject.Remove(guid.ToString());

            _addressBookServiceMock.Verify(s => s.Delete(guid.ToString()));
        }

        [Fact]
        public void SetPrimaryShippingAddress_ShouldCallSetPreferredShippingAddressOnService()
        {
            var guid = Guid.NewGuid();

            _subject.SetPreferredShippingAddress(guid.ToString());

            _addressBookServiceMock.Verify(s => s.SetPreferredShippingAddress(guid.ToString()));
        }

        [Fact]
        public void SetPrimaryBillingAddress_ShouldCallSetPreferredBillingAddressOnService()
        {
            var guid = Guid.NewGuid();

            _subject.SetPreferredBillingAddress(guid.ToString());

            _addressBookServiceMock.Verify(s => s.SetPreferredBillingAddress(guid.ToString()));
        }

        [Fact]
        public void OnException_ShouldDelegateToExceptionHandler()
        {
            var testController = CreateTestController();
            testController.CallOnException(_exceptionContext);

            _controllerExceptionHandlerMock.Verify(x => x.HandleRequestValidationException(_exceptionContext, "save", testController.OnSaveException));
        }

        [Fact]
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

            Assert.IsType<AddressViewModel>(((ViewResult)result).Model);
        }

        [Fact]
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
            Assert.NotNull(result);
        }

        [Fact]
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
            Assert.Equal<Guid?>(guid, Guid.Parse(model.Address.AddressId));
        }

        [Fact]
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
            Assert.Equal<AddressBookPage>(currentPage, model.CurrentPage);

            Assert.IsType<EmptyResult>(_exceptionContext.Result);
        }

        [Fact]
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

            Assert.IsType<AddressViewModel>(((ViewResult)result).Model);
        }

        [Fact]
        public void OnSaveException_WhenActionIsSaveAndAjaxRequest_ShouldCreatePartialView()
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
                Setup_AjaxRequest();
                Setup_action_for_controller("save", _subject);
            }

            var result = _subject.OnSaveException(_exceptionContext);

            Assert.IsType<PartialViewResult>(result);
        }

        AddressBookController _subject;
        MemoryLocalizationService _localizationService;
        Mock<IContentLoader> _contentLoaderMock;
        Mock<IAddressBookService> _addressBookServiceMock;
        Mock<HttpRequestBase> _httpRequestBaseMock;
        Mock<HttpContextBase> _httpContextBaseMock;
        Mock<RequestContext> _requestContextMock;
        Mock<ControllerExceptionHandler> _controllerExceptionHandlerMock;
        ExceptionContext _exceptionContext;


        public AddressBookControllerTests()
        {
            _controllerExceptionHandlerMock = new Mock<ControllerExceptionHandler>();
            _requestContextMock = new Mock<RequestContext>();
            _httpRequestBaseMock = new Mock<HttpRequestBase>();


            _httpContextBaseMock = new Mock<HttpContextBase>();
            _httpContextBaseMock.SetupGet(x => x.Request).Returns(_httpRequestBaseMock.Object);

            _exceptionContext = new ExceptionContext
            {
                HttpContext = _httpContextBaseMock.Object,
                RequestContext = _requestContextMock.Object
            };

            _contentLoaderMock = new Mock<IContentLoader>();
            _addressBookServiceMock = new Mock<IAddressBookService>();
            _addressBookServiceMock.Setup(x => x.CanSave(It.IsAny<AddressModel>())).Returns(true);

            _contentLoaderMock.Setup(c => c.Get<StartPage>(ContentReference.StartPage)).Returns(new StartPage());

            _localizationService = new MemoryLocalizationService();
            _localizationService.AddString(CultureInfo.CreateSpecificCulture("en"), "/AddressBook/Form/Error/ExistingAddress", "error");

            _subject = new AddressBookController(_contentLoaderMock.Object, _addressBookServiceMock.Object, _localizationService, _controllerExceptionHandlerMock.Object);
            _subject.ControllerContext = new ControllerContext(_httpContextBaseMock.Object, new RouteData(), _subject);
        }

        private AddressBookControllerForTest CreateTestController()
        {
            return new AddressBookControllerForTest(_contentLoaderMock.Object, _addressBookServiceMock.Object, _localizationService, _controllerExceptionHandlerMock.Object);
        }

        private void Setup_action_for_controller(string actionName, Controller controller)
        {
            controller.ControllerContext.RouteData.Values.Add("action", actionName);
        }

        private void Setup_RequestContext_to_contain_routed_data(object rotedData)
        {
            var routeData = new RouteData();
            routeData.DataTokens.Add(RoutingConstants.RoutedDataKey, rotedData);

            _requestContextMock.Setup(x => x.RouteData).Returns(routeData);
        }

        private void Setup_AjaxRequest()
        {
            // Simulate that requests are sent using AJAX.
            _httpRequestBaseMock.SetupGet(x => x.Headers).Returns(new System.Net.WebHeaderCollection { { "X-Requested-With", "XMLHttpRequest" } });
        }

        private void Setup_exception(Exception exception)
        {
            _exceptionContext.Exception = exception;
        }

        private void Setup_form_on_HttpRequestBase(NameValueCollection nameValueCollection)
        {
            _httpRequestBaseMock.Setup(x => x.Form).Returns(nameValueCollection);
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

