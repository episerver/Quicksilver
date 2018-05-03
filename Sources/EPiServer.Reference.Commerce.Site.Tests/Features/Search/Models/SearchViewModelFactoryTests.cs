using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.Product.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Search.Models;
using EPiServer.Reference.Commerce.Site.Features.Search.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Search.ViewModels;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Mediachase.Search;
using Xunit;
using ISearchService = EPiServer.Reference.Commerce.Site.Features.Search.Services.ISearchService;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Search.Models
{
    public class SearchViewModelFactoryTests
    {
        [Fact]
        public void Create_WhenQStartsWithLetter_ShouldNotReportAsError()
        {
            // Arrange
            var formModel = new FilterOptionViewModel() { Q = "query" };

            // Act
            var result = _subject.Create<IContent>(null, formModel);

            // Assert
            Assert.False(result.HasError);
        }

        [Fact]
        public void Create_WhenQStartsWithQuestionmark_ShouldReportAsError()
        {
            // Arrange
            var formModel = new FilterOptionViewModel() { Q = "?query" };
            
            // Act
            var result = _subject.Create<IContent>(null, formModel); 

            // Assert
            Assert.True(result.HasError);
        }

        [Fact]
        public void Create_WhenQStartsWithStar_ShouldReportAsError()
        {
            // Arrange
            var formModel = new FilterOptionViewModel() { Q = "*query" };
            
            // Act
            var result = _subject.Create<IContent>(null, formModel);

            // Assert
            Assert.True(result.HasError);
        }

        [Fact]
        public void Create_WhenPassingContent_ShouldUseItAsCurrentContent()
        {
            // Arrange
            var formModel = new FilterOptionViewModel() { Q = "query" };
            var content = new Mock<IContent>().Object;

            // Act
            var result = _subject.Create<IContent>(content, formModel);

            // Assert
            Assert.Equal<IContent>(content, result.CurrentContent);
        }

        [Fact]
        public void Create_WhenPassingFormModel_ShouldUseItAsFormModel()
        {
            // Arrange
            var formModel = new FilterOptionViewModel() { Q = "query" };

            // Act
            var result = _subject.Create<IContent>(null, formModel);

            // Assert
            Assert.Equal<FilterOptionViewModel>(formModel, result.FilterOption);
        }

        [Fact]
        public void Create_WhenSearching_ShouldGetProductViewModelsFromSearchResult()
        {
            // Arrange
            var formModel = new FilterOptionViewModel() { Q = "query" };

            // Act
            var result = _subject.Create<IContent>(null, formModel);

            // Assert
            Assert.Equal<IEnumerable<ProductTileViewModel>>(_productViewModels, result.ProductViewModels);
        }

        [Fact]
        public void Create_WhenSearching_ShouldGetFacetsFromSearchResult()
        {
            // Arrange
            var formModel = new FilterOptionViewModel() { Q = "query" };

            // Act
            var result = _subject.Create<IContent>(null, formModel);

            // Assert
            Assert.Equal<ISearchFacetGroup[]>(_facetGroups, result.Facets);
        }

        [Fact]
        public void Create_WhenSearching_ShouldSetTotalCountOnFormModel()
        {
            // Arrange
            var formModel = new FilterOptionViewModel() { Q = "query" };
            _searchResultsMock
                .Setup(s => s.TotalCount)
                .Returns(666);

            // Act
            var result = _subject.Create<IContent>(null, formModel);

            // Assert
            Assert.Equal<int>(666, result.FilterOption.TotalCount);
        }

        SearchViewModelFactory _subject;
        Mock<ISearchService> _searchServiceMock;
        Mock<ISearchResults> _searchResultsMock;
        IEnumerable<ProductTileViewModel> _productViewModels = Enumerable.Empty<ProductTileViewModel>();
        ISearchFacetGroup[] _facetGroups = new ISearchFacetGroup[0];

        public SearchViewModelFactoryTests()
        {
            _searchServiceMock = new Mock<ISearchService>();
            _searchResultsMock = new Mock<ISearchResults>();

            _searchResultsMock
                .Setup(s => s.FacetGroups)
                .Returns(_facetGroups);

            _searchServiceMock
                .Setup(s => s.Search(It.IsAny<IContent>(), It.IsAny<FilterOptionViewModel>()))
                .Returns(new CustomSearchResult() { 
                    FacetGroups = Enumerable.Empty<FacetGroupOption>(),
                    ProductViewModels = _productViewModels,
                    SearchResult = _searchResultsMock.Object});

            _subject = new SearchViewModelFactory(new MemoryLocalizationService(), _searchServiceMock.Object);
        }
    }
}
