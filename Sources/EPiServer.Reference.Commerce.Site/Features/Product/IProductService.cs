using System.Collections.Generic;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;

namespace EPiServer.Reference.Commerce.Site.Features.Product
{
    public interface IProductService
    {
        IEnumerable<ProductViewModel> GetVariationsAndPricesForProducts(IEnumerable<ProductContent> products);
        ProductViewModel GetProductViewModel(ProductContent product);
        ProductViewModel GetProductViewModel(VariationContent variation);
    }
}