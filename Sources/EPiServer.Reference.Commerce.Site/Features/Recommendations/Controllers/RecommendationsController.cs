using EPiServer.Personalization.Commerce.Tracking;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Services;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Recommendations.Controllers
{
    public class RecommendationsController : Controller
    {
        private readonly IRecommendationService _recommendationService;

        public RecommendationsController(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        [ChildActionOnly]
        public ActionResult Index(IEnumerable<Recommendation> recommendations)
        {
            if (recommendations == null || !recommendations.Any())
            {
                return new EmptyResult();
            }

            var viewModel = new RecommendationsViewModel
            {
                Products = _recommendationService.GetRecommendedProductTileViewModels(recommendations)
            };

            return PartialView("_Recommendations", viewModel);
        }
    }
}