using EPiServer.Core;
using EPiServer.Recommendations.Commerce.Tracking;
using EPiServer.Reference.Commerce.Site.Features.Product.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Services;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.ViewModels;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Recommendations.Controllers
{
    public class RecommendationsControllerTests
    {
        [Fact]
        public void Index_WhenThereAreNoEntryLinks_ShouldReturnEmptyResult()
        {
            var result = _subject.Index(Enumerable.Empty<Recommendation>());

            Assert.Equal(typeof(EmptyResult), result.GetType());
        }

        [Fact]
        public void Index_WhenThereAreMultipleEntryLinks_ShouldCreateProductViewModelForEach()
        {
            var recommendations = new Recommendation[] {
                new Recommendation(123, new ContentReference(1337)),
                new Recommendation(456, new ContentReference(1338))
            };

            _recommendationServiceMock
                .Setup(mock => mock.GetRecommendedProductTileViewModels(It.IsAny<IEnumerable<Recommendation>>()))
                .Returns(recommendations.Select(item => new RecommendedProductTileViewModel(item.RecommendationId, new ProductTileViewModel())));

            var result = (PartialViewResult)_subject.Index(recommendations);

            Assert.Equal(recommendations.Length, ((RecommendationsViewModel)result.Model).Products.Count());
        }

        private readonly Mock<IRecommendationService> _recommendationServiceMock;
        private readonly RecommendationsController _subject;

        public RecommendationsControllerTests()
        {
            _recommendationServiceMock = new Mock<IRecommendationService>();
            _subject = new RecommendationsController(_recommendationServiceMock.Object);
        }
    }
}