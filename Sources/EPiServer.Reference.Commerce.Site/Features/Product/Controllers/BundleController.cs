using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Product.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Extensions;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Tracking.Commerce;
using EPiServer.Web.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Services;
using Mediachase.Commerce.Catalog;

namespace EPiServer.Reference.Commerce.Site.Features.Product.Controllers
{
    public class BundleController : ContentController<BundleContent>
    {
        private readonly bool _isInEditMode;
        private readonly ReferenceConverter _referenceConverter;
        private readonly CatalogEntryViewModelFactory _viewModelFactory;
        private readonly IRecommendationService _recommendationService;

        public BundleController(IsInEditModeAccessor isInEditModeAccessor, CatalogEntryViewModelFactory viewModelFactory, IRecommendationService recommendationService, ReferenceConverter referenceConverter)
        {
            _isInEditMode = isInEditModeAccessor();
            _viewModelFactory = viewModelFactory;
            _recommendationService = recommendationService;
            _referenceConverter = referenceConverter;
        }

        [HttpGet]
        public async Task<ActionResult> Index(FashionBundle currentContent, bool useQuickview = false)
        {
            var viewModel = _viewModelFactory.Create(currentContent);

            if (_isInEditMode && !viewModel.Entries.Any())
            {
                var emptyViewName = "BundleWithoutEntries";
                return Request.IsAjaxRequest() ? PartialView(emptyViewName, viewModel) : (ActionResult)View(emptyViewName, viewModel);
            }

            var trackingResults = await _recommendationService.TrackProductAsync(HttpContext, currentContent.Code, useQuickview);

            if (useQuickview)
            {
                return PartialView("_Quickview", viewModel);
            }

            viewModel.AlternativeProducts = trackingResults?.GetAlternativeProductsRecommendations(_referenceConverter).Take(3);
            viewModel.CrossSellProducts = trackingResults?.GetCrossSellProductsRecommendations(_referenceConverter);

            return Request.IsAjaxRequest() ? PartialView(viewModel) : (ActionResult)View(viewModel);
        }
    }
}