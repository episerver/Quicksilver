using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Cart.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Cart.Models;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Product.Services;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using FluentAssertions;
using Mediachase.Commerce;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Cart.Controllers
{
    [TestClass]
    public class CartControllerTests
    {
        [TestMethod]
        public void MiniCartDetails_WhenCreatingViewModel_ShouldCreateModel()
        {
            var expectedResult = new MiniCartViewModel
            {
                ItemCount = 6,
                CheckoutPage = new ContentReference(444),
                CartItems = new List<CartItem>
                {
                    new CartItem
                    {
                        Code = "code",
                        DiscountPrice = new Money(45, Currency.USD),
                        DisplayName = "red",
                        ExtendedPrice = new Money(270, Currency.USD),
                        PlacedPrice = new Money(50, Currency.USD),
                        Url = "url",
                        Quantity = 6
                    }
                },
                Total = new Money(270, Currency.USD)
            };
            var result = ((PartialViewResult)_subject.MiniCartDetails()).Model as MiniCartViewModel;
            result.ShouldBeEquivalentTo(expectedResult);
        }

        [TestMethod]
        public void LargeCart_WhenCreatingViewModel_ShouldCreateModel()
        {
            var expectedResult = new LargeCartViewModel
            {
                CartItems = new List<CartItem>
                {
                    new CartItem
                    {
                        Code = "code",
                        DiscountPrice = new Money(45, Currency.USD),
                        DisplayName = "red",
                        ExtendedPrice = new Money(270, Currency.USD),
                        PlacedPrice = new Money(50, Currency.USD),
                        Url = "url",
                        Quantity = 6
                    }
                },
                Total = new Money(270, Currency.USD),
                TotalDiscount = new Money(30, Currency.USD)
            };
            var result = ((PartialViewResult)_subject.LargeCart()).Model as LargeCartViewModel;
            result.ShouldBeEquivalentTo(expectedResult);
        }

        [TestMethod]
        public void AddToCart_WhenAddingToCart_ShouldCallRemoveItemOnWishlistService()
        {
            _subject.AddToCart("Code 1");
            _wishListServiceMock.Verify(s => s.RemoveLineItem("Code 1"));
        }

        [TestMethod]
        public void ChangeCartItem_WhenChangeQuantity_ShouldCallChangeQuantityOnCartService()
        {
            _subject.ChangeCartItem("Code 1", 7, null, null);
            _cartServiceMock.Verify(s => s.ChangeQuantity("Code 1", 7));
        }

        [TestMethod]
        public void ChangeCartItem_WhenQuantityIsZero_ShouldCallRemoveLineItemOnCartService()
        {
            _subject.ChangeCartItem("Code 1", 0, null, null);
            _cartServiceMock.Verify(s => s.RemoveLineItem("Code 1"));
        }

        CartController _subject;
        Mock<IContentLoader> _contentLoaderMock;
        Mock<ICartService> _cartServiceMock;
        Mock<IProductService> _ProductServiceMock;
        Mock<ICartService> _wishListServiceMock;

        [TestInitialize]
        public void Setup()
        {
            string warningMessage = null;
            _contentLoaderMock = new Mock<IContentLoader>();
            _cartServiceMock = new Mock<ICartService>();
            _ProductServiceMock = new Mock<IProductService>();
            _wishListServiceMock = new Mock<ICartService>();

            _contentLoaderMock.Setup(c => c.Get<StartPage>(ContentReference.StartPage))
                .Returns(new StartPage
                {
                    CheckoutPage = new ContentReference(444)
                });

            _cartServiceMock.Setup(x => x.GetCartItems())
                .Returns(new List<CartItem>
                {
                    new CartItem
                    {
                        Code = "code",
                        DiscountPrice = new Money(45, Currency.USD),
                        DisplayName = "red",
                        ExtendedPrice = new Money(270, Currency.USD),
                        PlacedPrice = new Money(50, Currency.USD),
                        Url = "url",
                        Quantity = 6
                    }
                });

            _cartServiceMock.Setup(x => x.ConvertToMoney(270)).Returns(new Money(270, Currency.USD));
            _cartServiceMock.Setup(x => x.GetLineItemsTotalQuantity()).Returns(6);
            _cartServiceMock.Setup(x => x.GetTotal()).Returns(new Money(270, Currency.USD));
            _cartServiceMock.Setup(x => x.GetSubTotal()).Returns(new Money(270, Currency.USD));
            _cartServiceMock.Setup(x => x.GetTotalDiscount()).Returns(new Money(30, Currency.USD));
            _cartServiceMock.Setup(x => x.AddToCart(It.IsAny<string>(), out warningMessage)).Returns(true).Verifiable();
            _cartServiceMock.Setup(x => x.RemoveLineItem(It.IsAny<string>())).Verifiable();
            _cartServiceMock.Setup(x => x.ChangeQuantity(It.IsAny<string>(), It.IsAny<int>())).Verifiable();
            _wishListServiceMock.Setup(x => x.RemoveLineItem(It.IsAny<string>())).Verifiable();
            _subject = new CartController(_contentLoaderMock.Object, _cartServiceMock.Object, _wishListServiceMock.Object, _ProductServiceMock.Object);
        }
    }
}
