using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Internal;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes
{
    public class FakeCart : InMemoryOrderGroup, ICart
    {
        public FakeCart(IOrderGroup orderGroup) : base(orderGroup)
        {
        }

        public FakeCart(IMarket market, Currency currency) : base(market, currency)
        {
        }
    }
}
