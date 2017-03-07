using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Product.Services;
using EPiServer.Reference.Commerce.Site.Features.Product.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Controllers;
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
            var result = _subject.Index(new ContentReference[0]);

            Assert.Equal(typeof(EmptyResult), result.GetType());
        }

        [Fact]
        public void Index_WhenThereAreMultipleEntryLinks_ShouldCreateProductViewModelForEach()
        {
            var entryLinks = new[] { new ContentReference(1337), new ContentReference(1338) };
            
            _productServiceMock
                .Setup(mock => mock.GetProductTileViewModels(It.IsAny<IEnumerable<ContentReference>>()))
                .Returns(entryLinks.Select(link => new ProductTileViewModel()));

            var result = (PartialViewResult)_subject.Index(entryLinks);

            Assert.Equal(entryLinks.Length, ((RecommendationsViewModel)result.Model).Products.Count());
        }

        private readonly Mock<IProductService> _productServiceMock;
        private readonly RecommendationsController _subject;

        public RecommendationsControllerTests()
        {
            _productServiceMock = new Mock<IProductService>();
            _subject = new RecommendationsController(_productServiceMock.Object);
        }
    }
}