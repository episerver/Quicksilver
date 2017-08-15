using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Product.ViewModels;
using System.Collections.Generic;
using EPiServer.Core;

namespace EPiServer.Reference.Commerce.Site.Features.Product.Services
{
    public interface IProductService
    {
        ProductTileViewModel GetProductTileViewModel(EntryContentBase entry);
        ProductTileViewModel GetProductTileViewModel(ContentReference contentLink);
        string GetSiblingVariantCodeBySize(string siblingCode, string size);
    }
}