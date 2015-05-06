using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.Reference.Commerce.Site.Features.Product.Blocks;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Product.Controllers
{
    public class ProductListBlockController : BlockController<ProductListBlock>
    {
        private readonly IContentLoader _contentLoader;
        private readonly ProductService _productService;
        private readonly CultureInfo _preferredCulture;

        public ProductListBlockController(IContentLoader contentLoader, ProductService productService)
        {
            _contentLoader = contentLoader;
            _productService = productService;
            _preferredCulture = ContentLanguage.PreferredCulture;
        }

        [HttpGet]
        public override ActionResult Index(ProductListBlock currentBlock)
        {

            var items = currentBlock.Products != null ? currentBlock.Products.Items : new List<ContentAreaItem>();
            var products = _contentLoader.GetItems(items
                                         .Select(x => x.ContentLink).ToList(), _preferredCulture)
                                         .OfType<ProductContent>();            
            
            var model = new ProductListBlockViewModel
            {
                CurrentBlock = currentBlock,
                Products = _productService.GetVariationsAndPricesForProducts(products)
            };

            return PartialView(model);
        }
    }
}