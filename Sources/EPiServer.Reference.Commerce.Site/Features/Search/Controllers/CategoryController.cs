using System.Threading.Tasks;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Services;
using EPiServer.Reference.Commerce.Site.Features.Search.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Search.ViewModels;
using EPiServer.Web.Mvc;
using Mediachase.Commerce.Catalog;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Search.Controllers
{
    public class CategoryController : ContentController<FashionNode>
    {
        private readonly SearchViewModelFactory _viewModelFactory;
        private readonly IRecommendationService _recommendationService;
        private readonly ReferenceConverter _referenceConverter;

        public CategoryController(
            SearchViewModelFactory viewModelFactory,
            IRecommendationService recommendationService,
            ReferenceConverter referenceConverter)
        {
            _viewModelFactory = viewModelFactory;
            _recommendationService = recommendationService;
            _referenceConverter = referenceConverter;
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ViewResult> Index(FashionNode currentContent, FilterOptionViewModel viewModel)
        {
            var model = _viewModelFactory.Create(currentContent, viewModel);

            if (HttpContext.Request.HttpMethod == "GET")
            {
                var trackingResult = await _recommendationService.TrackCategoryAsync(HttpContext, currentContent);
                model.Recommendations = trackingResult.GetCategoryRecommendations(_referenceConverter);
            }

            return View(model);
        }

        [ChildActionOnly]
        public ActionResult Facet(FashionNode currentContent, FilterOptionViewModel viewModel)
        {
            return PartialView("_Facet", viewModel);
        }
    }
}