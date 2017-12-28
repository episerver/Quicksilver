using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Pages;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
using System;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.AddressBook.Controllers
{
    [Authorize]
    public class AddressBookController : PageController<AddressBookPage>
    {
        private readonly IContentLoader _contentLoader;
        private readonly IAddressBookService _addressBookService;
        private readonly LocalizationService _localizationService;
        private readonly ControllerExceptionHandler _controllerExceptionHandler;

        public AddressBookController(
            IContentLoader contentLoader,
            IAddressBookService addressBookService,
            LocalizationService localizationService,
            ControllerExceptionHandler controllerExceptionHandler)
        {
            _contentLoader = contentLoader;
            _addressBookService = addressBookService;
            _localizationService = localizationService;
            _controllerExceptionHandler = controllerExceptionHandler;
        }

        [HttpGet]
        public ActionResult Index(AddressBookPage currentPage)
        {
            AddressCollectionViewModel viewModel = _addressBookService.GetAddressBookViewModel(currentPage);

            return View(viewModel);
        }

        [HttpGet]
        public ActionResult EditForm(AddressBookPage currentPage, string addressId)
        {
            AddressViewModel viewModel = new AddressViewModel
            {
                Address = new AddressModel
                {
                    AddressId = addressId,
                },
                CurrentPage = currentPage
            };

            _addressBookService.LoadAddress(viewModel.Address);

            return AddressEditView(viewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetRegionsForCountry(string countryCode, string region, string htmlPrefix)
        {
            ViewData.TemplateInfo.HtmlFieldPrefix = htmlPrefix;
            var countryRegion = new CountryRegionViewModel
            {
                RegionOptions = _addressBookService.GetRegionsByCountryCode(countryCode),
                Region = region
            };

            return PartialView("~/Views/Shared/EditorTemplates/AddressRegion.cshtml", countryRegion);
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult Save(AddressViewModel viewModel)
        {
            if (String.IsNullOrEmpty(viewModel.Address.Name))
            {
                ModelState.AddModelError("Address.Name", _localizationService.GetString("/Shared/Address/Form/Empty/Name"));
            }

            if (!_addressBookService.CanSave(viewModel.Address))
            {
                ModelState.AddModelError("Address.Name", _localizationService.GetString("/AddressBook/Form/Error/ExistingAddress"));
            }

            if (!ModelState.IsValid)
            {
                _addressBookService.LoadAddress(viewModel.Address);

                return AddressEditView(viewModel);
            }

            _addressBookService.Save(viewModel.Address);

            if (Request.IsAjaxRequest())
            {
                return Json(viewModel.Address);
            }

            return RedirectToAction("Index", new { node = GetStartPage().AddressBookPage });
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult Remove(string addressId)
        {
            _addressBookService.Delete(addressId);
            return RedirectToAction("Index", new { node = GetStartPage().AddressBookPage });
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult SetPreferredShippingAddress(string addressId)
        {
            _addressBookService.SetPreferredShippingAddress(addressId);
            return RedirectToAction("Index", new { node = GetStartPage().AddressBookPage });
        }

        [HttpPost]
        [AllowDBWrite]
        public ActionResult SetPreferredBillingAddress(string addressId)
        {
            _addressBookService.SetPreferredBillingAddress(addressId);
            return RedirectToAction("Index", new { node = GetStartPage().AddressBookPage });
        }

        public ActionResult OnSaveException(ExceptionContext filterContext)
        {
            var currentPage = filterContext.RequestContext.GetRoutedData<AddressBookPage>();

            var viewModel = new AddressViewModel
            {
                Address = new AddressModel
                {
                    AddressId = filterContext.HttpContext.Request.Form["addressId"],
                    ErrorMessage = filterContext.Exception.Message,
                },
                CurrentPage = currentPage
            };

            _addressBookService.LoadAddress(viewModel.Address);

            return AddressEditView(viewModel);
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            _controllerExceptionHandler.HandleRequestValidationException(filterContext, "save", OnSaveException);
        }

        private ActionResult AddressEditView(AddressViewModel viewModel)
        {
            if (Request.IsAjaxRequest())
            {
                return PartialView("ModalAddressDialog", viewModel);
            }

            return View("EditForm", viewModel);
        }

        private StartPage GetStartPage()
        {
            return _contentLoader.Get<StartPage>(ContentReference.StartPage);
        }
    }
}