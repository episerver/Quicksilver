using EPiServer.Reference.Commerce.Site.Features.Product.ViewModels;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Recommendations.ViewModels
{
    public class RecommendationsViewModel
    {
        public IEnumerable<ProductTileViewModel> Products { get; set; }
    }
}