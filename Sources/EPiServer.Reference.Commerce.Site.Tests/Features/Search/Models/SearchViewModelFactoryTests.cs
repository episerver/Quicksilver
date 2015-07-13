using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPiServer.Reference.Commerce.Site.Features.Search.Models;
using Moq;
using EPiServer.Framework.Localization;
using EPiServer.Core;
using System.Linq;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using Mediachase.Search;
using System.Collections.Generic;
using EPiServer.Reference.Commerce.Site.Features.Search.Services;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Search.Models
{
    [TestClass]
    public class SearchViewModelFactoryTests
    {
        [TestMethod]
        public void Create_WhenQStartsWithLetter_ShouldNotReportAsError()
        {
            // Arrange
            var formModel = new FilterOptionFormModel() { Q = "query" };

            // Act
            var result = _subject.Create<IContent>(null, formModel);

            // Assert
            Assert.IsFalse(result.HasError);
        }

        [TestMethod]
        public void Create_WhenQStartsWithQuestionmark_ShouldReportAsError()
        {
            // Arrange
            var formModel = new FilterOptionFormModel() { Q = "?query" };
            
            // Act
            var result = _subject.Create<IContent>(null, formModel); 

            // Assert
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        public void Create_WhenQStartsWithStar_ShouldReportAsError()
        {
            // Arrange
            var formModel = new FilterOptionFormModel() { Q = "*query" };
            
            // Act
            var result = _subject.Create<IContent>(null, formModel);

            // Assert
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        public void Create_WhenPassingContent_ShouldUseItAsCurrentContent()
        {
            // Arrange
            var formModel = new FilterOptionFormModel() { Q = "query" };
            var content = new Mock<IContent>().Object;

            // Act
            var result = _subject.Create<IContent>(content, formModel);

            // Assert
            Assert.AreEqual<IContent>(content, result.CurrentContent);
        }

        [TestMethod]
        public void Create_WhenPassingFormModel_ShouldUseItAsFormModel()
        {
            // Arrange
            var formModel = new FilterOptionFormModel() { Q = "query" };

            // Act
            var result = _subject.Create<IContent>(null, formModel);

            // Assert
            Assert.AreEqual<FilterOptionFormModel>(formModel, result.FormModel);
        }

        [TestMethod]
        public void Create_WhenSearching_ShouldGetProductViewModelsFromSearchResult()
        {
            // Arrange
            var formModel = new FilterOptionFormModel() { Q = "query" };

            // Act
            var result = _subject.Create<IContent>(null, formModel);

            // Assert
            Assert.AreEqual<IEnumerable<ProductViewModel>>(_productViewModels, result.ProductViewModels);
        }

        [TestMethod]
        public void Create_WhenSearching_ShouldGetFacetsFromSearchResult()
        {
            // Arrange
            var formModel = new FilterOptionFormModel() { Q = "query" };

            // Act
            var result = _subject.Create<IContent>(null, formModel);

            // Assert
            Assert.AreEqual<ISearchFacetGroup[]>(_facetGroups, result.Facets);
        }

        [TestMethod]
        public void Create_WhenSearching_ShouldSetTotalCountOnFormModel()
        {
            // Arrange
            var formModel = new FilterOptionFormModel() { Q = "query" };
            _searchResultsMock
                .Setup(s => s.TotalCount)
                .Returns(666);

            // Act
            var result = _subject.Create<IContent>(null, formModel);

            // Assert
            Assert.AreEqual<int>(666, result.FormModel.TotalCount);
        }

        SearchViewModelFactory _subject;
        Mock<ISearchService> _searchServiceMock;
        Mock<ISearchResults> _searchResultsMock;
        IEnumerable<ProductViewModel> _productViewModels = Enumerable.Empty<ProductViewModel>();
        ISearchFacetGroup[] _facetGroups = new ISearchFacetGroup[0];

        [TestInitialize]
        public void Setup()
        {
            _searchServiceMock = new Mock<ISearchService>();
            _searchResultsMock = new Mock<ISearchResults>();

            _searchResultsMock
                .Setup(s => s.FacetGroups)
                .Returns(_facetGroups);

            _searchServiceMock
                .Setup(s => s.Search(It.IsAny<IContent>(), It.IsAny<FilterOptionFormModel>()))
                .Returns(new CustomSearchResult() { 
                    FacetGroups = Enumerable.Empty<FacetGroupOption>(),
                    ProductViewModels = _productViewModels,
                    SearchResult = _searchResultsMock.Object});

            _subject = new SearchViewModelFactory(new MemoryLocalizationService(), _searchServiceMock.Object);
        }
    }
}
