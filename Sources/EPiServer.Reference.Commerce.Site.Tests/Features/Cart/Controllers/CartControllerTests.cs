using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Cart;
using EPiServer.Reference.Commerce.Site.Features.Cart.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Cart.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
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
        public void AddToCart_WhenAddingToCart_ShouldCallAddToCartOnCartService()
        {
            _subject.AddToCart("Code 1");
            _cartServiceMock.Verify(s => s.AddToCart("Code 1"));
        }

        [TestMethod]
        public void AddToCart_WhenAddingToCart_ShouldCallRemoveItemOnWishlistService()
        {
            _subject.AddToCart("Code 1");
            _wishListServiceMock.Verify(s => s.RemoveItem("Code 1"));
        }

        [TestMethod]
        public void ChangeQuantity_WhenChangeQuantity_ShouldCallChangeQuantityOnCartService()
        {
            _subject.ChangeQuantity("Code 1", 7);
            _cartServiceMock.Verify(s => s.ChangeQuantity("Code 1", 7));
        }

        [TestMethod]
        public void ChangeQuantity_WhenMiniCartTrue_ShouldRedirectMiniCartDetails()
        {
            var result = _subject.ChangeQuantity("Code 1", 7, true);
            var redirectResult = result as RedirectToRouteResult;
            Assert.AreEqual<string>("MiniCartDetails", (string)redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void ChangeQuantity_WhenMiniCartIsFalse_ShouldRedirectLargeCart()
        {
            var result = _subject.ChangeQuantity("Code 1", 7);
            var redirectResult = result as RedirectToRouteResult;
            Assert.AreEqual<string>("LargeCart", (string)redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void RemoveLineItem_WhenRemoveLineItem_ShouldCallRemoveLineItemOnCartService()
        {
            _subject.RemoveLineItem("Code 1");
            _cartServiceMock.Verify(s => s.RemoveLineItem("Code 1"));
        }

        [TestMethod]
        public void RemoveLineItem_WhenRemoveLineItem_ShouldRedirectLargeCart()
        {
            var result = _subject.RemoveLineItem("Code 1");
            var redirectResult = result as RedirectToRouteResult;
            Assert.AreEqual<string>("LargeCart", (string)redirectResult.RouteValues["action"]);
        }

        CartController _subject;
        Mock<IContentLoader> _contentLoaderMock;
        Mock<ICartService> _cartServiceMock;
        Mock<IWishListService> _wishListServiceMock;

        [TestInitialize]
        public void Setup()
        {
            _contentLoaderMock = new Mock<IContentLoader>();
            _cartServiceMock = new Mock<ICartService>();
            _wishListServiceMock = new Mock<IWishListService>();

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
            _cartServiceMock.Setup(x => x.GetTotalDiscount()).Returns(new Money(30, Currency.USD));
            _cartServiceMock.Setup(x => x.AddToCart(It.IsAny<string>())).Verifiable();
            _cartServiceMock.Setup(x => x.RemoveLineItem(It.IsAny<string>())).Verifiable();
            _cartServiceMock.Setup(x => x.ChangeQuantity(It.IsAny<string>(), It.IsAny<int>())).Verifiable();
            _wishListServiceMock.Setup(x => x.RemoveItem(It.IsAny<string>())).Verifiable();
            _subject = new CartController(_contentLoaderMock.Object, _cartServiceMock.Object, _wishListServiceMock.Object);
        }
    }
}
