using EPiServer.Recommendations.Tracking;
using EPiServer.Recommendations.Tracking.Data;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Product.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Web.Mvc;
using Mediachase.Commerce.Catalog;
using System.Linq;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Product.Controllers
{
    public class ProductController : ContentController<FashionProduct>
    {
        private readonly bool _isInEditMode;
        private readonly CatalogEntryViewModelFactory _viewModelFactory;
        private readonly IRecommendationService _recommendationService;
        private readonly ReferenceConverter _referenceConverter;

        public ProductController(IsInEditModeAccessor isInEditModeAccessor, CatalogEntryViewModelFactory viewModelFactory, IRecommendationService recommendationService, ReferenceConverter referenceConverter)
        {
            _isInEditMode = isInEditModeAccessor();
            _viewModelFactory = viewModelFactory;
            _recommendationService = recommendationService;
            _referenceConverter = referenceConverter;
        }

        [HttpGet]
        public ActionResult Index(FashionProduct currentContent, string entryCode = "", bool useQuickview = false, bool skipTracking = false)
        {
            var viewModel = _viewModelFactory.Create(currentContent, entryCode);
           
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
                if (!skipTracking)
                {
                    _recommendationService.SendProductTracking(HttpContext, currentContent.Code, RetrieveRecommendationMode.Disabled);
                }

                return PartialView("_Quickview", viewModel);
            }

            var trackingResponse = new TrackingResponseData();
            if (!skipTracking)
            {
                trackingResponse = _recommendationService.SendProductTracking(HttpContext, currentContent.Code, RetrieveRecommendationMode.Enabled);
            }

            viewModel.AlternativeProducts = trackingResponse.GetAlternativeProductsRecommendations(_referenceConverter).Take(3);
            viewModel.CrossSellProducts = trackingResponse.GetCrossSellProductsRecommendations(_referenceConverter);

            return Request.IsAjaxRequest() ? PartialView(viewModel) : (ActionResult)View(viewModel);
        }

        [HttpPost]
        public ActionResult SelectVariant(FashionProduct currentContent, string color, string size, bool useQuickview = false)
        {
            var variant = _viewModelFactory.SelectVariant(currentContent, color, size);
            if (variant != null)
            {
                return RedirectToAction("Index", new { entryCode = variant.Code, useQuickview = useQuickview, skipTracking = true });
            }

            return HttpNotFound();

        }
    }
}