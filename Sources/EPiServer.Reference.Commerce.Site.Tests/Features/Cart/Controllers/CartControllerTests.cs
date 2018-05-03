using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using EPiServer.Reference.Commerce.Site.Features.Cart.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Services;
using Moq;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Cart.Controllers
{
    public class CartControllerTests
    {
        [Fact]
        public void MiniCartDetails_ShouldCreateViewModel()
        {
            _subject.MiniCartDetails();
            _cartViewModelFactoryMock.Verify(s => s.CreateMiniCartViewModel(It.IsAny<ICart>()));
        }

        [Fact]
        public void LargeCart_ShouldCreateViewModel()
        {
            _subject.LargeCart();
            _cartViewModelFactoryMock.Verify(s => s.CreateLargeCartViewModel(It.IsAny<ICart>()));
        }

        [Fact]
        public async Task ChangeCartItem_WhenChangeQuantity_ShouldCallChangeCartItemOnCartService()
        {
            string code = "Code 1";
            int shipmentId = 1;
            int quantity = 0;
            string size = null;
            string newSize = null;
            await _subject.ChangeCartItem(shipmentId, code, quantity, size, newSize, "");
            _cartServiceMock.Verify(s => s.ChangeCartItem(It.IsAny<ICart>(), shipmentId, code, quantity, size, newSize, ""));
        }

        [Fact]
        public async Task AddToCart_ShouldCallAddToCartOnCartService()
        {
            string code = "Code 1";

            await _subject.AddToCart(code);
            _cartServiceMock.Verify(s => s.AddToCart(It.IsAny<ICart>(), code, 1));
        }

        [Fact]
        public async Task AddToCart_WhenSuccessfullyAdded_ShouldSaveCart()
        {
            string code = "Code 1";
            await _subject.AddToCart(code);
            _orderRepositoryMock.Verify(s => s.Save(It.IsAny<IOrderGroup>()), Times.Once);
        }

        [Fact]
        public async Task AddToCart_WhenFailedToAdd_ShouldNotSaveCart()
        {
            string code = "Non-existing-code";

            _cartServiceMock
                .Setup(x => x.AddToCart(It.IsAny<ICart>(), It.IsAny<string>(), It.IsAny<decimal>()))
                .Returns(new AddToCartResult
                {
                    EntriesAddedToCart = false
                })
                .Verifiable();

            await _subject.AddToCart(code);
            _orderRepositoryMock.Verify(s => s.Save(It.IsAny<IOrderGroup>()), Times.Never);
        }

        private readonly CartController _subject;
        private readonly Mock<ICartService> _cartServiceMock;
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<CartViewModelFactory> _cartViewModelFactoryMock;

        public CartControllerTests()
        {
            _cartServiceMock = new Mock<ICartService>();
            _cartViewModelFactoryMock = new Mock<CartViewModelFactory>(null, null, null, null, null);
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _cartServiceMock
                .Setup(x => x.AddToCart(It.IsAny<ICart>(), "Code 1", It.IsAny<decimal>()))
                .Returns((ICart cart, string code, decimal quantity) =>
                {
                    return new AddToCartResult
                    {
                        EntriesAddedToCart = true
                    };
                })
                .Verifiable();
            _subject = new CartController(_cartServiceMock.Object, _orderRepositoryMock.Object, Mock.Of<IRecommendationService>(), _cartViewModelFactoryMock.Object);
        }
    }
}