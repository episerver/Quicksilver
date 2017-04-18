using EPiServer.Reference.Commerce.Site.Features.Product.ViewModels;

namespace EPiServer.Reference.Commerce.Site.Features.Recommendations.ViewModels
{
    public class RecommendedProductTileViewModel
    {
        public string RecommendationId { get; }

        public ProductTileViewModel TileViewModel { get; }

        public RecommendedProductTileViewModel(string recommendationId, ProductTileViewModel model)
        {
            RecommendationId = recommendationId;
            TileViewModel = model;
        }
    }
}