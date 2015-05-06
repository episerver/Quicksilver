using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Models;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Pages;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Web.Mvc;
using System;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.AddressBook.Controllers
{
    [Authorize]
    public class AddressBookController : PageController<AddressBookPage>
    {
        private readonly IContentLoader _contentLoader;
        private readonly IAddressBookService _addressBookService;

        public AddressBookController(IContentLoader contentLoader,
            IAddressBookService addressBookService)
        {
            _contentLoader = contentLoader;
            _addressBookService = addressBookService;
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
        public ActionResult EditForm(AddressBookFormModel formModel)
        {
            var model = _addressBookService.LoadFormModel(formModel);
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Save(AddressBookFormModel formModel)
        {
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
        public ActionResult SetPrimaryShippingAddress(Guid addressId)
        {
            _addressBookService.SetPrimaryShippingAddress(addressId);
            return RedirectToAction("Index", new { node = GetStartPage().AddressBookPage });
        }

        [HttpPost]
        public ActionResult SetPrimaryBillingAddress(Guid addressId)
        {
            _addressBookService.SetPrimaryBillingAddress(addressId);
            return RedirectToAction("Index", new { node = GetStartPage().AddressBookPage });
        }

        private StartPage GetStartPage()
        {
            return _contentLoader.Get<StartPage>(ContentReference.StartPage);
        }
    }
}