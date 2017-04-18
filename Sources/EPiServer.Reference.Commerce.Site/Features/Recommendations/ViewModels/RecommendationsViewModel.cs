using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Recommendations.ViewModels
{
    public class RecommendationsViewModel
    {
        public IEnumerable<RecommendedProductTileViewModel> Products { get; set; }
    }
}