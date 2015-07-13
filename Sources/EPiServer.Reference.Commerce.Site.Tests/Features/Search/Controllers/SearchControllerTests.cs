using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Search.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Search.Models;
using EPiServer.Reference.Commerce.Site.Features.Search.Pages;
using EPiServer.Reference.Commerce.Site.Features.Search.Services;
using FluentAssertions;
using Mediachase.Commerce;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Search.Controllers
{
    [TestClass]
    public class SearchControllerTests
    {
        [TestMethod]
        public void Index_ShouldReturnViewModel()
        {
            var result = ((ViewResult)_subject.Index(new SearchPage(), new FilterOptionFormModel())).Model as SearchViewModel<SearchPage>;
            var expectedResult = new SearchViewModel<SearchPage> 
            {
                ErrorMessage = "Error"
            };
            result.ShouldBeEquivalentTo(expectedResult);
        }

        [TestMethod]
        public void QuickSearch_WhenSearch_ShouldReturnIEnumerableProductViewModel()
        {
            var result = ((ViewResult)_subject.QuickSearch("test")).Model as ProductViewModel[];
            var expectedResult = new [] 
            { 
                new ProductViewModel
                {
                    DisplayName = "Test",
                    PlacedPrice = new Money(10m, Currency.USD),
                    ExtendedPrice = new Money(10m, Currency.USD)
                }
            };
            result.ShouldAllBeEquivalentTo(expectedResult);
        }

        SearchController _subject;
        Mock<SearchViewModelFactory> _searchViwModelFactoryMock;
        Mock<ISearchService> _searchServiceMock;

        [TestInitialize]
        public void Setup()
        {
            _searchServiceMock = new Mock<ISearchService>();
            _searchViwModelFactoryMock = new Mock<SearchViewModelFactory>(null,null);

            _searchViwModelFactoryMock.Setup(
                x => x.Create(
                    It.IsAny<SearchPage>(), It.IsAny<FilterOptionFormModel>()))
                .Returns((SearchPage content, FilterOptionFormModel model) => new SearchViewModel<SearchPage> 
                {
                    ErrorMessage = "Error"
                });

            _searchServiceMock.Setup(x => x.QuickSearch("test"))
                .Returns(new[] 
                { 
                    new ProductViewModel
                    {
                        DisplayName = "Test",
                        PlacedPrice = new Money(10m, Currency.USD),
                        ExtendedPrice = new Money(10m, Currency.USD)
                    }
                });
            _subject = new SearchController(_searchViwModelFactoryMock.Object, _searchServiceMock.Object);

        }
    }
}
