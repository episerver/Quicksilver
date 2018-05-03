using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Cart.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Cart.Pages;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Services;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes;
using Mediachase.Commerce;
using Moq;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Cart.Controllers
{
    public class WishListControllerTests
    {
        [Fact]
        public void WishListMiniCartDetails_ShouldCreateViewModel()
        {
            _subject.WishListMiniCartDetails();
            _cartViewModelFactoryMock.Verify(s => s.CreateWishListMiniCartViewModel(It.IsAny<ICart>()));
        }

        [Fact]
        public void Index_ShouldCreateViewModel()
        {
            _subject.Index(new WishListPage());
            _cartViewModelFactoryMock.Verify(s => s.CreateWishListViewModel(It.IsAny<ICart>()));
        }

        [Fact]
        public async Task ChangeCartItem_WhenChangeQuantity_ShouldCallChangeCartItemOnCartService()
        {
            string code = "Code 1";
            int quantity = 0;
            string size = null;
            string newSize = null;
            await _subject.ChangeCartItem(code, quantity, size, newSize, "");
            _cartServiceMock.Verify(s => s.ChangeCartItem(It.IsAny<ICart>(), 0, code, quantity, size, newSize, ""));
        }

        [Fact]
        public async Task AdddToCart_ShouldCallAddToCartOnCartService()
        {
            string code = "Code 1";

            await _subject.AddToCart(code);
            _cartServiceMock.Verify(s => s.AddToCart(It.IsAny<ICart>(), code, 1));
        }

        [Fact]
        public async Task AdddToCart_WhenSuccessfullyAdded_ShouldSaveCart()
        {
            string code = "Code 1";

            await _subject.AddToCart(code);
            _orderRepositoryMock.Verify(s => s.Save(It.IsAny<IOrderGroup>()), Times.Once);
        }

        [Fact]
        public async Task AdddToCart_WhenFailedToAdd_ShouldNotSaveCart()
        {
            string code = "Non-existing-code";

            _cartServiceMock
                .Setup(x => x.AddToCart(It.IsAny<ICart>(), It.IsAny<string>(), It.IsAny<decimal>()))
                .Returns(new AddToCartResult());
            await _subject.AddToCart(code);
            _orderRepositoryMock.Verify(s => s.Save(It.IsAny<IOrderGroup>()), Times.Never);
        }

        [Fact]
        public void DeleteWishList()
        {
            _subject.DeleteWishList();
        }

        private readonly WishListController _subject;
        private readonly Mock<IMarket> _marketMock;
        private readonly Mock<ICartService> _cartServiceMock;
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<CartViewModelFactory> _cartViewModelFactoryMock;
        private readonly Mock<IContentLoader> _contentLoaderMock;

        public WishListControllerTests()
        {
            _marketMock = new Mock<IMarket>();
            _cartServiceMock = new Mock<ICartService>();
            _cartViewModelFactoryMock = new Mock<CartViewModelFactory>(null, null, null, null, null);
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _contentLoaderMock = new Mock<IContentLoader>();

            _cartViewModelFactoryMock.Setup(x => x.CreateWishListMiniCartViewModel(It.IsAny<ICart>())).Returns(new WishListMiniCartViewModel());
            _cartViewModelFactoryMock.Setup(x => x.CreateWishListViewModel(It.IsAny<ICart>())).Returns(new WishListViewModel());
            _contentLoaderMock.Setup(x => x.Get<StartPage>(ContentReference.StartPage)).Returns(new StartPage());
            _cartServiceMock.Setup(x => x.LoadOrCreateCart(It.IsAny<string>())).Returns(new FakeCart(_marketMock.Object, new Currency("USD")));
            _cartServiceMock
                .Setup(x => x.AddToCart(It.IsAny<ICart>(), It.IsAny<string>(), It.IsAny<decimal>()))
                .Returns((ICart cart, string code, decimal quantity) =>
                {
                    return new AddToCartResult
                    {
                        EntriesAddedToCart = true
                    };
                })
                .Verifiable();

            _subject = new WishListController(_contentLoaderMock.Object, _cartServiceMock.Object, _orderRepositoryMock.Object, Mock.Of<IRecommendationService>(), _cartViewModelFactoryMock.Object);
        }
    }
}
