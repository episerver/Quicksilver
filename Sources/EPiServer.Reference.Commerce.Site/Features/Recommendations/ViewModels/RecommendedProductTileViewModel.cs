using EPiServer.Reference.Commerce.Site.Features.Product.ViewModels;

namespace EPiServer.Reference.Commerce.Site.Features.Recommendations.ViewModels
{
    public class RecommendedProductTileViewModel
    {
        public long RecommendationId { get; }

        public ProductTileViewModel TileViewModel { get; }

        public RecommendedProductTileViewModel(long recommendationId, ProductTileViewModel model)
        {
            RecommendationId = recommendationId;
            TileViewModel = model;
        }
    }
}