using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Search.Models;
using EPiServer.Reference.Commerce.Site.Features.Search.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.Web.Routing;
using FluentAssertions;
using Lucene.Net.Util;
using Mediachase.Commerce;
using Mediachase.Search;
using Mediachase.Search.Extensions;
using Moq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using EPiServer.Globalization;
using EPiServer.Reference.Commerce.Site.Features.Product.ViewModels;
using Xunit;
using EPiServer.Reference.Commerce.Site.Features.Search.ViewModels;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Search
{

    public class SearchServiceTests
    {
        [Fact]
        public void Search_WhenFilterOptionsIsNull_ShouldReturnEmptyResult()
        {
            var content = new NodeContent();
            var result = _subject.Search(content, null);

            result.ProductViewModels.Should().BeEmpty();
            result.FacetGroups.Should().BeEmpty();
            result.SearchResult.FacetGroups.Should().BeEmpty();
        }

        [Fact]
        public void Search_ShouldReturnSameSearchResult()
        {
            var content = new NodeContent();
            var filterOptions = new FilterOptionViewModel { FacetGroups = new List<FacetGroupOption>() };

            var result = _subject.Search(content, filterOptions);

            result.SearchResult.Should().BeEquivalentTo(_searchResultsMock.Object);
        }

        [Fact]
        public void Search_ShouldReturnPopulatedProductViewModel()
        {
            var content = new NodeContent();
            var filterOptions = new FilterOptionViewModel { FacetGroups = new List<FacetGroupOption>() };

            var result = _subject.Search(content, filterOptions);

            var productViewModel = result.ProductViewModels.First();

            var expected = new ProductTileViewModel
            {
                DisplayName = "DisplayName",
                PlacedPrice = new Money(1, _currentCurrency),
                DiscountedPrice = new Money(1, _currentCurrency),
                ImageUrl = "/image.jpg",
                Url = "http://domain.com",
                Code = "Code",
                Brand = "Brand",
                IsAvailable = true
            };

            productViewModel.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Search_ShouldReturnPopulatedFacetGroupOption()
        {
            var content = new NodeContent();
            var filterOptions = new FilterOptionViewModel { FacetGroups = new List<FacetGroupOption>() };

            var result = _subject.Search(content, filterOptions);

            var facetGroupOption = result.FacetGroups.First();

            var expected = new FacetGroupOption
            {
                GroupName = "FacetGroupName",
                GroupFieldName = "FacetGroupFieldName",
                Facets = new List<FacetOption>
                {
                    new FacetOption
                    {
                        Name = _facet.Name,
                        Key = _facet.Key,
                        Selected = _facet.IsSelected,
                        Count = _facet.Count
                    }
                }
            };

            facetGroupOption.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void QuickSearch_WhenQueryIsEmpty_ShouldReturnEmptyList()
        {
            var filterOptions = new FilterOptionViewModel();

            var result = _subject.QuickSearch(filterOptions);

            result.Should().BeEmpty();
        }

        [Fact]
        public void QuickSearch_ShouldReturnPopulatedProductViewModel()
        {
            var filterOptions = new FilterOptionViewModel();
            filterOptions.Q = "query";

            var result = _subject.QuickSearch(filterOptions);

            var productViewModel = result.First();

            var expected = new ProductTileViewModel
            {
                DisplayName = "DisplayName",
                PlacedPrice = new Money(1, _currentCurrency),
                DiscountedPrice = new Money(1, _currentCurrency),
                ImageUrl = "/image.jpg",
                Url = "http://domain.com",
                Brand = "Brand",
                Code = "Code",
                IsAvailable = true
            };

            productViewModel.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void QuickSearch_ShouldFilterByCurrentMarket()
        {
            var filterOptions = new FilterOptionViewModel { Q = "query" };
            _subject.QuickSearch(filterOptions);

            var expected = _currentMarketMock.Object.GetCurrentMarket().MarketId;

            _searchFacadeMock.Verify(x => x.Search(It.Is<CatalogEntrySearchCriteria>(y => y.MarketId.Equals(expected))));
        }

        [Fact]
        public void QuickSearch_WhenQueryContainsPlusCharacter_ShouldRemovePlusCharacterFromQuery()
        {
            const string searchQuery = "start+end";
            const string expectedResult = "startend*";

            var filterOptions = new FilterOptionViewModel { Q = searchQuery };
            _subject.QuickSearch(filterOptions);

            _searchFacadeMock.Verify(x => x.Search(It.Is<CatalogEntrySearchCriteria>(y => y.SearchPhrase.Equals(expectedResult))));
        }

        [Fact]
        public void QuickSearch_WhenQueryContainsExclamationCharacter_ShouldRemoveExclamationCharacterFromQuery()
        {
            const string searchQuery = "start!end";
            const string expectedResult = "startend*";

            var filterOptions = new FilterOptionViewModel { Q = searchQuery };
            _subject.QuickSearch(filterOptions);

            _searchFacadeMock.Verify(x => x.Search(It.Is<CatalogEntrySearchCriteria>(y => y.SearchPhrase.Equals(expectedResult))));
        }

        [Fact]
        public void QuickSearch_WhenQueryContainsStartBracketCharacter_ShouldRemoveStartBracketCharacterFromQuery()
        {
            const string searchQuery = "start[end";
            const string expectedResult = "startend*";

            var filterOptions = new FilterOptionViewModel { Q = searchQuery };
            _subject.QuickSearch(filterOptions);

            _searchFacadeMock.Verify(x => x.Search(It.Is<CatalogEntrySearchCriteria>(y => y.SearchPhrase.Equals(expectedResult))));
        }

        [Fact]
        public void QuickSearch_WhenQueryContainsEndBracketCharacter_ShouldRemoveEndBracketCharacterFromQuery()
        {
            const string searchQuery = "start]end";
            const string expectedResult = "startend*";

            var filterOptions = new FilterOptionViewModel { Q = searchQuery };
            _subject.QuickSearch(filterOptions);

            _searchFacadeMock.Verify(x => x.Search(It.Is<CatalogEntrySearchCriteria>(y => y.SearchPhrase.Equals(expectedResult))));
        }

        [Fact]
        public void QuickSearch_WhenQueryContainsColonCharacter_ShouldRemoveColonCharacterFromQuery()
        {
            const string searchQuery = "start:end";
            const string expectedResult = "startend*";

            var filterOptions = new FilterOptionViewModel { Q = searchQuery };
            _subject.QuickSearch(filterOptions);

            _searchFacadeMock.Verify(x => x.Search(It.Is<CatalogEntrySearchCriteria>(y => y.SearchPhrase.Equals(expectedResult))));
        }

        [Fact]
        public void QuickSearch_WhenQueryContainsQuestionMarkCharacter_ShouldRemoveQuestionMarkCharacterFromQuery()
        {
            const string searchQuery = "start?end";
            const string expectedResult = "startend*";

            var filterOptions = new FilterOptionViewModel { Q = searchQuery };
            _subject.QuickSearch(filterOptions);

            _searchFacadeMock.Verify(x => x.Search(It.Is<CatalogEntrySearchCriteria>(y => y.SearchPhrase.Equals(expectedResult))));
        }

        [Fact]
        public void QuickSearch_WhenQueryContainsStartGullwingCharacter_ShouldRemoveStartGullwingCharacterFromQuery()
        {
            const string searchQuery = "start{end";
            const string expectedResult = "startend*";

            var filterOptions = new FilterOptionViewModel { Q = searchQuery };
            _subject.QuickSearch(filterOptions);

            _searchFacadeMock.Verify(x => x.Search(It.Is<CatalogEntrySearchCriteria>(y => y.SearchPhrase.Equals(expectedResult))));
        }

        [Fact]
        public void QuickSearch_WhenQueryContainsEndGullwingCharacter_ShouldRemoveEndGullwingCharacterFromQuery()
        {
            const string searchQuery = "start}end";
            const string expectedResult = "startend*";

            var filterOptions = new FilterOptionViewModel { Q = searchQuery };
            _subject.QuickSearch(filterOptions);

            _searchFacadeMock.Verify(x => x.Search(It.Is<CatalogEntrySearchCriteria>(y => y.SearchPhrase.Equals(expectedResult))));
        }

        [Fact]
        public void QuickSearch_WhenQueryContainsStarCharacter_ShouldRemoveStarCharacterFromQuery()
        {
            const string searchQuery = "start*end";
            const string expectedResult = "startend*";

            var filterOptions = new FilterOptionViewModel { Q = searchQuery };
            _subject.QuickSearch(filterOptions);

            _searchFacadeMock.Verify(x => x.Search(It.Is<CatalogEntrySearchCriteria>(y => y.SearchPhrase.Equals(expectedResult))));
        }

        [Fact]
        public void QuickSearch_WhenQueryContainsAndCharacter_ShouldRemoveAndCharacterFromQuery()
        {
            const string searchQuery = "start&end";
            const string expectedResult = "startend*";

            var filterOptions = new FilterOptionViewModel { Q = searchQuery };
            _subject.QuickSearch(filterOptions);

            _searchFacadeMock.Verify(x => x.Search(It.Is<CatalogEntrySearchCriteria>(y => y.SearchPhrase.Equals(expectedResult))));
        }

        [Fact]
        public void QuickSearch_WhenQueryContainsSeparatorCharacter_ShouldRemoveSeparatorCharacterFromQuery()
        {
            const string searchQuery = "start|end";
            const string expectedResult = "startend*";

            var filterOptions = new FilterOptionViewModel { Q = searchQuery };
            _subject.QuickSearch(filterOptions);

            _searchFacadeMock.Verify(x => x.Search(It.Is<CatalogEntrySearchCriteria>(y => y.SearchPhrase.Equals(expectedResult))));
        }

        [Fact]
        public void QuickSearch_WhenQueryContainsWaveCharacter_ShouldRemoveWaveCharacterFromQuery()
        {
            const string searchQuery = "start~end";
            const string expectedResult = "startend*";

            var filterOptions = new FilterOptionViewModel { Q = searchQuery };
            _subject.QuickSearch(filterOptions);

            _searchFacadeMock.Verify(x => x.Search(It.Is<CatalogEntrySearchCriteria>(y => y.SearchPhrase.Equals(expectedResult))));
        }

        [Fact]
        public void Search_ShouldFilterByCurrentMarket()
        {
            var filterOptions = new FilterOptionViewModel { Q = "query", FacetGroups = new List<FacetGroupOption>() };
            var content = new NodeContent();
            _subject.Search(content, filterOptions);

            var expected = _currentMarketMock.Object.GetCurrentMarket().MarketId;

            _searchFacadeMock.Verify(x => x.Search(It.Is<CatalogEntrySearchCriteria>(y => y.MarketId.Equals(expected))));
        }

        [Fact]
        public void Search_WhenQueryContainsPlusCharacter_ShouldRemovePlusCharacterFromQuery()
        {
            const string searchQuery = "start+end";
            const string expectedResult = "startend*";

            var content = new NodeContent();
            var filterOptions = new FilterOptionViewModel { Q = searchQuery, FacetGroups = new List<FacetGroupOption>() };
            _subject.Search(content, filterOptions);

            _searchFacadeMock.Verify(x => x.Search(It.Is<CatalogEntrySearchCriteria>(y => y.SearchPhrase.Equals(expectedResult))));
        }

        [Fact]
        public void Search_WhenQueryContainsExclamationCharacter_ShouldRemoveExclamationCharacterFromQuery()
        {
            const string searchQuery = "start!end";
            const string expectedResult = "startend*";

            var content = new NodeContent();
            var filterOptions = new FilterOptionViewModel { Q = searchQuery, FacetGroups = new List<FacetGroupOption>() };
            _subject.Search(content, filterOptions);

            _searchFacadeMock.Verify(x => x.Search(It.Is<CatalogEntrySearchCriteria>(y => y.SearchPhrase.Equals(expectedResult))));
        }

        [Fact]
        public void Search_WhenQueryContainsStartBracketCharacter_ShouldRemoveStartBracketCharacterFromQuery()
        {
            const string searchQuery = "start[end";
            const string expectedResult = "startend*";

            var content = new NodeContent();
            var filterOptions = new FilterOptionViewModel { Q = searchQuery, FacetGroups = new List<FacetGroupOption>() };
            _subject.Search(content, filterOptions);

            _searchFacadeMock.Verify(x => x.Search(It.Is<CatalogEntrySearchCriteria>(y => y.SearchPhrase.Equals(expectedResult))));
        }

        [Fact]
        public void Search_WhenQueryContainsEndBracketCharacter_ShouldRemoveEndBracketCharacterFromQuery()
        {
            const string searchQuery = "start]end";
            const string expectedResult = "startend*";

            var content = new NodeContent();
            var filterOptions = new FilterOptionViewModel { Q = searchQuery, FacetGroups = new List<FacetGroupOption>() };
            _subject.Search(content, filterOptions);

            _searchFacadeMock.Verify(x => x.Search(It.Is<CatalogEntrySearchCriteria>(y => y.SearchPhrase.Equals(expectedResult))));
        }

        [Fact]
        public void Search_WhenQueryContainsColonCharacter_ShouldRemoveColonCharacterFromQuery()
        {
            const string searchQuery = "start:end";
            const string expectedResult = "startend*";

            var content = new NodeContent();
            var filterOptions = new FilterOptionViewModel { Q = searchQuery, FacetGroups = new List<FacetGroupOption>() };
            _subject.Search(content, filterOptions);

            _searchFacadeMock.Verify(x => x.Search(It.Is<CatalogEntrySearchCriteria>(y => y.SearchPhrase.Equals(expectedResult))));
        }

        [Fact]
        public void Search_WhenQueryContainsQuestionMarkCharacter_ShouldRemoveQuestionMarkCharacterFromQuery()
        {
            const string searchQuery = "start?end";
            const string expectedResult = "startend*";

            var content = new NodeContent();
            var filterOptions = new FilterOptionViewModel { Q = searchQuery, FacetGroups = new List<FacetGroupOption>() };
            _subject.Search(content, filterOptions);

            _searchFacadeMock.Verify(x => x.Search(It.Is<CatalogEntrySearchCriteria>(y => y.SearchPhrase.Equals(expectedResult))));
        }

        [Fact]
        public void Search_WhenQueryContainsStartGullwingCharacter_ShouldRemoveStartGullwingCharacterFromQuery()
        {
            const string searchQuery = "start{end";
            const string expectedResult = "startend*";

            var content = new NodeContent();
            var filterOptions = new FilterOptionViewModel { Q = searchQuery, FacetGroups = new List<FacetGroupOption>() };
            _subject.Search(content, filterOptions);

            _searchFacadeMock.Verify(x => x.Search(It.Is<CatalogEntrySearchCriteria>(y => y.SearchPhrase.Equals(expectedResult))));
        }

        [Fact]
        public void Search_WhenQueryContainsEndGullwingCharacter_ShouldRemoveEndGullwingCharacterFromQuery()
        {
            const string searchQuery = "start}end";
            const string expectedResult = "startend*";

            var content = new NodeContent();
            var filterOptions = new FilterOptionViewModel { Q = searchQuery, FacetGroups = new List<FacetGroupOption>() };
            _subject.Search(content, filterOptions);

            _searchFacadeMock.Verify(x => x.Search(It.Is<CatalogEntrySearchCriteria>(y => y.SearchPhrase.Equals(expectedResult))));
        }

        [Fact]
        public void Search_WhenQueryContainsStarCharacter_ShouldRemoveStarCharacterFromQuery()
        {
            const string searchQuery = "start*end";
            const string expectedResult = "startend*";

            var content = new NodeContent();
            var filterOptions = new FilterOptionViewModel { Q = searchQuery, FacetGroups = new List<FacetGroupOption>() };
            _subject.Search(content, filterOptions);

            _searchFacadeMock.Verify(x => x.Search(It.Is<CatalogEntrySearchCriteria>(y => y.SearchPhrase.Equals(expectedResult))));
        }

        [Fact]
        public void Search_WhenQueryContainsAndCharacter_ShouldRemoveAndCharacterFromQuery()
        {
            const string searchQuery = "start&end";
            const string expectedResult = "startend*";

            var content = new NodeContent();
            var filterOptions = new FilterOptionViewModel { Q = searchQuery, FacetGroups = new List<FacetGroupOption>() };
            _subject.Search(content, filterOptions);

            _searchFacadeMock.Verify(x => x.Search(It.Is<CatalogEntrySearchCriteria>(y => y.SearchPhrase.Equals(expectedResult))));
        }

        [Fact]
        public void Search_WhenQueryContainsSeparatorCharacter_ShouldRemoveSeparatorCharacterFromQuery()
        {
            const string searchQuery = "start|end";
            const string expectedResult = "startend*";

            var content = new NodeContent();
            var filterOptions = new FilterOptionViewModel { Q = searchQuery, FacetGroups = new List<FacetGroupOption>() };
            _subject.Search(content, filterOptions);

            _searchFacadeMock.Verify(x => x.Search(It.Is<CatalogEntrySearchCriteria>(y => y.SearchPhrase.Equals(expectedResult))));
        }

        [Fact]
        public void Search_WhenQueryContainsWaveCharacter_ShouldRemoveWaveCharacterFromQuery()
        {
            const string searchQuery = "start~end";
            const string expectedResult = "startend*";

            var content = new NodeContent();
            var filterOptions = new FilterOptionViewModel { Q = searchQuery, FacetGroups = new List<FacetGroupOption>() };
            _subject.Search(content, filterOptions);

            _searchFacadeMock.Verify(x => x.Search(It.Is<CatalogEntrySearchCriteria>(y => y.SearchPhrase.Equals(expectedResult))));
        }

        private SearchService _subject;
        private Mock<ICurrentMarket> _currentMarketMock;
        private Mock<ICurrencyService> _currencyServiceMock;
        private Mock<ISearchResults> _searchResultsMock;
        private Mock<IContentLoader> _contentLoaderMock;
        private MemoryLocalizationService _localizationService;
        private Currency _currentCurrency;
        private Facet _facet;
        private Mock<SearchFacade> _searchFacadeMock;
        private Mock<LanguageResolver> _languageResolverMock;


        public SearchServiceTests()
        {
            SetupSearchResultMock();

            var marketMock = new Mock<IMarket>();
            marketMock.Setup(x => x.MarketId).Returns(new MarketId("Market"));
            _currentMarketMock = new Mock<ICurrentMarket>();
            _currentMarketMock.Setup(x => x.GetCurrentMarket()).Returns(marketMock.Object);

            _currentCurrency = new Currency("USD");
            _currencyServiceMock = new Mock<ICurrencyService>();
            _currencyServiceMock.Setup(x => x.GetCurrentCurrency()).Returns(_currentCurrency);

            var urlResolverMock = new Mock<UrlResolver>();
            urlResolverMock.Setup(x => x.GetUrl(It.IsAny<ContentReference>())).Returns("http://domain.com");

            _searchFacadeMock = new Mock<SearchFacade>();
            _searchFacadeMock.Setup(x => x.Search(It.IsAny<CatalogEntrySearchCriteria>())).Returns(_searchResultsMock.Object);
            _searchFacadeMock.Setup(x => x.SearchFilters).Returns(new[] { new SearchFilter() });
            _searchFacadeMock.Setup(x => x.GetOutlinesForNode(It.IsAny<string>())).Returns(new StringCollection());
            _searchFacadeMock.Setup(x => x.GetSearchProvider()).Returns(SearchFacade.SearchProviderType.Lucene);

            _languageResolverMock = new Mock<LanguageResolver>();
            _languageResolverMock.Setup(x => x.GetPreferredCulture()).Returns(CultureInfo.GetCultureInfo("en"));

            _localizationService = new MemoryLocalizationService();
            _localizationService.AddString(CultureInfo.GetCultureInfo("en"), "/Facet/Category", "Category");

            _contentLoaderMock = new Mock<IContentLoader>();
            _contentLoaderMock.Setup(x => x.GetChildren<NodeContent>(It.IsAny<ContentReference>())).Returns(new[] { new NodeContent()
            {
                DisplayName = "Node",
                Code = "Node"
            }});

            _subject = new SearchService(
                _currentMarketMock.Object,
                _currencyServiceMock.Object,
                urlResolverMock.Object,
                _searchFacadeMock.Object,
               _languageResolverMock.Object,
                _contentLoaderMock.Object,
                _localizationService);
        }

        private void SetupSearchResultMock()
        {
            var facetGroupMock = new Mock<ISearchFacetGroup>();
            facetGroupMock.Setup(x => x.Name).Returns("FacetGroupName");
            facetGroupMock.Setup(x => x.FieldName).Returns("FacetGroupFieldName");
            _facet = new Facet(facetGroupMock.Object, "FacetKey", "FacetName", 10);
            var facetList = new ISearchFacet[] { _facet };
            facetGroupMock.Setup(x => x.Facets).Returns(() => facetList);

            var searchDocumentMock = new Mock<ISearchDocument>();
            searchDocumentMock.Setup(x => x["displayname"])
                .Returns(() => new SearchField("displayname", "DisplayName"));
            searchDocumentMock.Setup(x => x["image_url"])
                .Returns(() => new SearchField("image_url", "/image.jpg"));  // Tests will ensures that hostname gets removed
            searchDocumentMock.Setup(x => x["content_link"])
                .Returns(() => new SearchField("content_link", "1"));
            searchDocumentMock.Setup(x => x[It.IsNotIn(new[] { "displayname", "image_url", "content_link" })])
                .Returns(() => new SearchField("name", ConvertToPrefixCodedLong(1m)));

            searchDocumentMock.Setup(x => x["brand"]).Returns(() => new SearchField("brand", "Brand"));
            searchDocumentMock.Setup(x => x["code"]).Returns(() => new SearchField("code", "Code"));

            _searchResultsMock = new Mock<ISearchResults>();
            _searchResultsMock.Setup(x => x.FacetGroups).Returns(() => new[] { facetGroupMock.Object });

            _searchResultsMock.Setup(x => x.Documents)
                .Returns(() => new SearchDocuments { searchDocumentMock.Object });
        }

        protected string ConvertToPrefixCodedLong(decimal input)
        {
            return NumericUtils.LongToPrefixCoded(long.Parse((input * 10000).ToString(CultureInfo.InvariantCulture)));
        }
    }
}
