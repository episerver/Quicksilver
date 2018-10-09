using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Search.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Search.ViewModels;
using EPiServer.Web.Mvc;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Search.Controllers
{
    public class CategoryController : ContentController<FashionNode>
    {
        private readonly SearchViewModelFactory _viewModelFactory;

        public CategoryController(
            SearchViewModelFactory viewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ViewResult Index(FashionNode currentContent, FilterOptionViewModel viewModel)
        {
            var model = _viewModelFactory.Create(currentContent, viewModel);
         
            return View(model);
        }

        [ChildActionOnly]
        public ActionResult Facet(FashionNode currentContent, FilterOptionViewModel viewModel)
        {
            return PartialView("_Facet", viewModel);
        }
    }
}