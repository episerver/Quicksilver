using EPiServer.Reference.Commerce.Site.Features.Product.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Search.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Search.Pages;
using EPiServer.Reference.Commerce.Site.Features.Search.Services;
using EPiServer.Reference.Commerce.Site.Features.Search.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Search.ViewModels;
using FluentAssertions;
using Mediachase.Commerce;
using Moq;
using System.Web.Mvc;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Search.Controllers
{
    public class SearchControllerTests
    {
        [Fact]
        public void Index_ShouldReturnViewModel()
        {
            var result = ((ViewResult)_subject.Index(new SearchPage(), new FilterOptionViewModel())).Model as SearchViewModel<SearchPage>;
            var expectedResult = new SearchViewModel<SearchPage> 
            {
                ErrorMessage = "Error"
            };
            result.ShouldBeEquivalentTo(expectedResult);
        }

        [Fact]
        public void QuickSearch_WhenSearch_ShouldReturnIEnumerableProductViewModel()
        {
            var result = ((ViewResult)_subject.QuickSearch("test")).Model as ProductViewModel[];
            var expectedResult = new [] 
            { 
                new ProductViewModel
                {
                    DisplayName = "Test",
                    PlacedPrice =  new Money(10, Currency.USD),
                    DiscountedPrice = new Money(10, Currency.USD)
                }
            };
            result.ShouldAllBeEquivalentTo(expectedResult);
        }

        SearchController _subject;
        Mock<SearchViewModelFactory> _searchViwModelFactoryMock;
        Mock<ISearchService> _searchServiceMock;

        public SearchControllerTests()
        {
            _searchServiceMock = new Mock<ISearchService>();
            _searchViwModelFactoryMock = new Mock<SearchViewModelFactory>(null,null);

            _searchViwModelFactoryMock.Setup(
                x => x.Create(
                    It.IsAny<SearchPage>(), It.IsAny<FilterOptionViewModel>()))
                .Returns((SearchPage content, FilterOptionViewModel model) => new SearchViewModel<SearchPage> 
                {
                    ErrorMessage = "Error"
                });

            _searchServiceMock.Setup(x => x.QuickSearch("test"))
                .Returns(new[] 
                { 
                    new ProductViewModel
                    {
                        DisplayName = "Test",
                        PlacedPrice =  new Money(10, Currency.USD),
                        DiscountedPrice = new Money(10, Currency.USD)
                    }
                });
            _subject = new SearchController(_searchViwModelFactoryMock.Object, _searchServiceMock.Object);

        }
    }
}
