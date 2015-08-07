using System.Collections.Generic;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;

namespace EPiServer.Reference.Commerce.Site.Features.Product.Services
{
    public interface IProductService
    {
        IEnumerable<ProductViewModel> GetVariationsAndPricesForProducts(IEnumerable<ProductContent> products);
        ProductViewModel GetProductViewModel(ProductContent product);
        ProductViewModel GetProductViewModel(VariationContent variation);
        string GetSiblingVariantCodeBySize(string siblingCode, string size);
        IEnumerable<FashionVariant> GetVariations(FashionProduct currentContent);
    }
}