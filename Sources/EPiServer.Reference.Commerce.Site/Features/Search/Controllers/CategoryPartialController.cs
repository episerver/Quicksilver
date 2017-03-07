using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Reference.Commerce.Site.Features.Product.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Search.Models;
using EPiServer.Reference.Commerce.Site.Features.Search.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Search.ViewModels;
using EPiServer.Web.Mvc;
using System.Collections.Generic;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Search.Controllers
{
    [TemplateDescriptor(Inherited = true)]
    public class CategoryPartialController : PartialContentController<NodeContent>
    {
        private readonly SearchViewModelFactory _viewModelFactory;

        public CategoryPartialController(SearchViewModelFactory viewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public override ActionResult Index(NodeContent currentContent)
        {
            var productModels = GetProductModels(currentContent);
            return PartialView("_Category", productModels);
        }

        protected IEnumerable<ProductTileViewModel> GetProductModels(NodeContent currentContent)
        {
            return GetSearchModel(currentContent, 3).ProductViewModels;
        }

        protected virtual SearchViewModel<NodeContent> GetSearchModel(NodeContent currentContent, int pageSize)
        {
            return _viewModelFactory.Create(currentContent, new FilterOptionViewModel
            {
                FacetGroups = new List<FacetGroupOption>(),
                Page = 1,
                PageSize = pageSize
            });
        }
    }
}