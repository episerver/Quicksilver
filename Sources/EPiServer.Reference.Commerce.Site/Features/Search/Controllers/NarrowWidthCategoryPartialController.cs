using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Search.Models;
using EPiServer.Reference.Commerce.Site.Infrastructure;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Search.Controllers
{
    [TemplateDescriptor(Inherited = true, Tags = new[] { Constants.ContentAreaTags.OneThirdWidth })]
    public class NarrowWidthCategoryPartialController : CategoryPartialController
    {
        public NarrowWidthCategoryPartialController(SearchViewModelFactory viewModelFactory)
            : base(viewModelFactory) { }

        protected override IEnumerable<ProductViewModel> GetProductModels(NodeContent currentContent)
        {
            return GetSearchModel(currentContent, NumberOfProducts).ProductViewModels;
        }

        public static readonly int NumberOfProducts = 1;
    }
}