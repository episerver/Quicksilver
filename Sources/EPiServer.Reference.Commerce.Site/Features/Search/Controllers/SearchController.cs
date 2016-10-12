using EPiServer.Reference.Commerce.Site.Features.Search.Pages;
using EPiServer.Reference.Commerce.Site.Features.Search.Services;
using EPiServer.Reference.Commerce.Site.Features.Search.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Search.ViewModels;
using EPiServer.Web.Mvc;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Search.Controllers
{
    public class SearchController : PageController<SearchPage>
    {
        private readonly SearchViewModelFactory _viewModelFactory;
        private readonly ISearchService _searchService;

        public SearchController(SearchViewModelFactory viewModelFactory, ISearchService searchService)
        {
            _viewModelFactory = viewModelFactory;
            _searchService = searchService;
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult Index(SearchPage currentPage, FilterOptionViewModel viewModel)
        {
            var model = _viewModelFactory.Create(currentPage, viewModel);

            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult QuickSearch(string q = "")
        {
            var result = _searchService.QuickSearch(q);
            return View(result);
        }
    }
}