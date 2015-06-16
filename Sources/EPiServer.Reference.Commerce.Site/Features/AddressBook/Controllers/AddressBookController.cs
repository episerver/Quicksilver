using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Models;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Pages;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Web.Mvc;
using System;
using System.Web.Mvc;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;

namespace EPiServer.Reference.Commerce.Site.Features.AddressBook.Controllers
{
    [Authorize]
    public class AddressBookController : PageController<AddressBookPage>
    {
        private readonly IContentLoader _contentLoader;
        private readonly IAddressBookService _addressBookService;
        private readonly LocalizationService _localizationService;

        public AddressBookController(IContentLoader contentLoader,
            IAddressBookService addressBookService, 
            LocalizationService localizationService)
        {
            _contentLoader = contentLoader;
            _addressBookService = addressBookService;
            _localizationService = localizationService;
        }

        [HttpGet]
        public ActionResult Index(AddressBookPage currentPage)
        {
            return View(_addressBookService.GetViewModel(currentPage));
        }

        [HttpGet]
        public ActionResult EditForm(AddressBookPage currentPage, Guid? addressId)
        {
            return View(_addressBookService.LoadFormModel(new AddressBookFormModel
            {
                AddressId = addressId,
                CurrentPage = currentPage
            }));
        }

        [HttpPost]
        public ActionResult GetRegionsForCountry(string countryCode, string region)
        {
            var viewModel = new AddressModelBase();
            viewModel.RegionOptions = _addressBookService.GetRegionOptionsByCountryCode(countryCode);
            viewModel.Region = region;

            return PartialView("_AddressRegion", viewModel);
        }

        [HttpPost]
        public ActionResult UpdateCountrySelection(AddressBookFormModel formModel)
        {
            _addressBookService.UpdateCountrySelection(formModel);
            return PartialView("EditForm", formModel);
        }

        [HttpPost]
        public ActionResult Save(AddressBookFormModel formModel)
        {
            if (!_addressBookService.CanSave(formModel))
            {
                ModelState.AddModelError("Name", _localizationService.GetString("/AddressBook/Form/Error/ExistingAddress"));
            }

            if (!ModelState.IsValid)
            {
                var model = _addressBookService.LoadFormModel(formModel);
                return View("EditForm", model);
            }

            _addressBookService.Save(formModel);
            return RedirectToAction("Index", new { node = GetStartPage().AddressBookPage });
        }

        [HttpPost]
        public ActionResult Remove(Guid addressId)
        {
            _addressBookService.Delete(addressId);
            return RedirectToAction("Index", new { node = GetStartPage().AddressBookPage });
        }

        [HttpPost]
        public ActionResult SetPreferredShippingAddress(Guid addressId)
        {
            _addressBookService.SetPreferredShippingAddress(addressId);
            return RedirectToAction("Index", new { node = GetStartPage().AddressBookPage });
        }

        [HttpPost]
        public ActionResult SetPreferredBillingAddress(Guid addressId)
        {
            _addressBookService.SetPreferredBillingAddress(addressId);
            return RedirectToAction("Index", new { node = GetStartPage().AddressBookPage });
        }

        private StartPage GetStartPage()
        {
            return _contentLoader.Get<StartPage>(ContentReference.StartPage);
        }
    }
}