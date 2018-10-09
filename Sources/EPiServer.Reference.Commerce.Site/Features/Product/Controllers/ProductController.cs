using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Product.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Web.Mvc;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Product.Controllers
{
    public class ProductController : ContentController<FashionProduct>
    {
        private readonly bool _isInEditMode;
        private readonly CatalogEntryViewModelFactory _viewModelFactory;

        public ProductController(IsInEditModeAccessor isInEditModeAccessor, CatalogEntryViewModelFactory viewModelFactory)
        {
            _isInEditMode = isInEditModeAccessor();
            _viewModelFactory = viewModelFactory;
        }

        [HttpGet]
        public ActionResult Index(FashionProduct currentContent, string entryCode = "", bool useQuickview = false, bool skipTracking = false)
        {
            var viewModel = _viewModelFactory.Create(currentContent, entryCode);
            viewModel.SkipTracking = skipTracking;

            if (_isInEditMode && viewModel.Variant == null)
            {
                var emptyViewName = "ProductWithoutEntries";
                return Request.IsAjaxRequest() ? PartialView(emptyViewName, viewModel) : (ActionResult)View(emptyViewName, viewModel);
            }

            if (viewModel.Variant == null)
            {
                return HttpNotFound();
            }

            if (useQuickview)
            {
                return PartialView("_Quickview", viewModel);
            }
            return Request.IsAjaxRequest() ? PartialView(viewModel) : (ActionResult)View(viewModel);
        }

        [HttpPost]
        public ActionResult SelectVariant(FashionProduct currentContent, string color, string size, bool useQuickview = false)
        {
            var variant = _viewModelFactory.SelectVariant(currentContent, color, size);
            if (variant != null)
            {
                return RedirectToAction("Index", new { entryCode = variant.Code, useQuickview, skipTracking = true });
            }

            return HttpNotFound();
        }
    }
}