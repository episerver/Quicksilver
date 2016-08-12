using EPiServer.Reference.Commerce.Site.Features.Cart.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Cart.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace EPiServer.Reference.Commerce.Site.Tests.Features.Cart.Extensions
{
    public class CartItemExtensionsTests
    {
        private IEnumerable<CartItem> _subject;

        public CartItemExtensionsTests()
        {
            _subject = new[]
            {
                new CartItem { Code = "1", AddressId = new Guid("352C89A8-6A58-44AD-B5E0-D1F455AD5B95"), Quantity = 1 },
                new CartItem { Code = "1", AddressId = new Guid("352C89A8-6A58-44AD-B5E0-D1F455AD5B95"), Quantity = 2 },
                new CartItem { Code = "1", AddressId = new Guid("352C89A8-6A58-44AD-B5E0-D1F455AD5B95"), Quantity = 3 },
            };
        }

        [Fact]
        public void ToFlattenArray_WhenMultipleQuantityForSameCartItem_ShouldReturnOneCartItemPerSku()
        {
            var result = _subject.ToFlattenedArray();

            Assert.Equal<int>(6, result.Length);
        }

        [Fact]
        public void AggregateCartItems_ShouldMergeCartItemsBasedOnSubjectsCode()
        {
            var additionalItems = new[]
            {
                new CartItem { Code = "1", PlacedPrice = 5, Quantity = 10 },
                new CartItem { Code = "2", PlacedPrice = 1, Quantity = 1 }
            };

            var result = _subject.AggregateCartItems(additionalItems);

            Assert.Equal<int>(1, result.Length);

            Assert.Equal<decimal>(6, result.Sum(x => x.Quantity));
        }
    }
}
