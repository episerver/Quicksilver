using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Product.Services;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Recommendations.Controllers
{
    public class RecommendationsController : Controller
    {
        private readonly IProductService _productService;

        public RecommendationsController(IProductService productService)
        {
            _productService = productService;
        }

        [ChildActionOnly]
        public ActionResult Index(IEnumerable<ContentReference> entryLinks)
        {
            if (!entryLinks.Any())
            {
                return new EmptyResult();
            }

            var viewModel = new RecommendationsViewModel
            {
                Products = _productService.GetProductTileViewModels(entryLinks)
            };

            return PartialView("_Recommendations", viewModel);
        }
    }
}