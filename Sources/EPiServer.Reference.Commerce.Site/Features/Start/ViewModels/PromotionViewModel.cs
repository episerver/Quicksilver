using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Marketing;
using EPiServer.Core;
using EPiServer.Web;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.Start.ViewModels
{
    public class PromotionViewModel
    {
        public string Name { get; set; }

        [UIHint(UIHint.Image)]
        public ContentReference BannerImage { get; set; }

        public IEnumerable<CatalogContentBase> Items { get; set; }

        public CatalogItemSelectionType SelectionType { get; set; }
    }
}