using EPiServer.Core;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Product.ViewModels
{
    public abstract class ProductViewModelBase
    {
        public IList<string> Images { get; set; }
        public IEnumerable<ContentReference> AlternativeProducts { get; set; }
        public IEnumerable<ContentReference> CrossSellProducts { get; set; }
    }
}