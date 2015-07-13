using System.Web.Mvc;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using EPiServer.Reference.Commerce.Site.Features.Product.Services;

namespace EPiServer.Reference.Commerce.Site.Features.Product.Controllers
{
    [TemplateDescriptor(Inherited = true)]
    public class ProductPartialController : PartialContentController<ProductContent>
    {
        private readonly IProductService _productService;

        public ProductPartialController(IProductService productService)
        {
            _productService = productService;
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public override ActionResult Index(ProductContent currentContent)
        {
            return PartialView("_Product", _productService.GetProductViewModel(currentContent));
        }
    }
}