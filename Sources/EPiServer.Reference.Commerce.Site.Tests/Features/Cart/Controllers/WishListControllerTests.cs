using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Cart.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Cart.Pages;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
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
        public void ChangeCartItem_WhenChangeQuantity_ShouldCallChangeCartItemOnCartService()
        {
            string code = "Code 1";
            int quantity = 0;
            string size = null;
            string newSize = null;
            _subject.ChangeCartItem(code, quantity, size, newSize);
            _cartServiceMock.Verify(s => s.ChangeCartItem(It.IsAny<ICart>(), 0, code, quantity, size, newSize));
        }

        [Fact]
        public void AdddToCart_ShouldCallAddToCartOnCartService()
        {
            string code = "Code 1";
            string warningMessage = null;

            _subject.AddToCart(code);
            _cartServiceMock.Verify(s => s.AddToCart(It.IsAny<ICart>(), code, out warningMessage));
        }

        [Fact]
        public void AdddToCart_WhenSuccessfullyAdded_ShouldSaveCart()
        {
            string code = "Code 1";
            _subject.AddToCart(code);
            _orderRepositoryMock.Verify(s => s.Save(It.IsAny<IOrderGroup>()), Times.Once);
        }

        [Fact]
        public void AdddToCart_WhenFailedToAdd_ShouldNotSaveCart()
        {
            string code = "Non-existing-code";
            _subject.AddToCart(code);
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
            string warningMessage = null;
            _marketMock = new Mock<IMarket>();
            _cartServiceMock = new Mock<ICartService>();
            _cartViewModelFactoryMock = new Mock<CartViewModelFactory>(null, null, null, null, null);
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _contentLoaderMock = new Mock<IContentLoader>();

            _cartViewModelFactoryMock.Setup(x => x.CreateWishListMiniCartViewModel(It.IsAny<ICart>())).Returns(new WishListMiniCartViewModel());
            _cartViewModelFactoryMock.Setup(x => x.CreateWishListViewModel(It.IsAny<ICart>())).Returns(new WishListViewModel());
            _cartServiceMock.Setup(x => x.AddToCart(It.IsAny<ICart>(), "Code 1", out warningMessage)).Returns(true).Verifiable();
            _cartServiceMock.Setup(x => x.LoadOrCreateCart(It.IsAny<string>())).Returns(new FakeCart(_marketMock.Object, new Currency("USD")));
            _contentLoaderMock.Setup(x => x.Get<StartPage>(ContentReference.StartPage)).Returns(new StartPage());

            _subject = new WishListController(_contentLoaderMock.Object, _cartServiceMock.Object, _orderRepositoryMock.Object, _cartViewModelFactoryMock.Object);
        }
    }
}
