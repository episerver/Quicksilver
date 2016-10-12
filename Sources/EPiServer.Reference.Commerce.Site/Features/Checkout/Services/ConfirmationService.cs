using System;
using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Internal;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.Services
{
    public class ConfirmationService
    {
        private readonly IOrderRepository _orderRepository;
        readonly ICurrentMarket _currentMarket;

        public ConfirmationService(
            IOrderRepository orderRepository,
            ICurrentMarket currentMarket)
        {
            _orderRepository = orderRepository;
            _currentMarket = currentMarket;
        }

        public IPurchaseOrder GetOrder(int orderNumber)
        {
            return _orderRepository.Load<IPurchaseOrder>(orderNumber);
        }

        public IPurchaseOrder CreateFakePurchaseOrder()
        {
            var form = new InMemoryOrderForm
            {
                Payments =
                {
                    new InMemoryPayment
                    {
                        BillingAddress = new InMemoryOrderAddress(),
                        PaymentMethodName = "CashOnDelivery"
                    }
                }
            };

            form.Shipments.First().ShippingAddress = new InMemoryOrderAddress();

            var purchaseOrder = new InMemoryPurchaseOrder
            {
                Forms = new[] { form },
                Currency = _currentMarket.GetCurrentMarket().DefaultCurrency,
                Market = _currentMarket.GetCurrentMarket(),
                OrderLink = new OrderReference(0, string.Empty, Guid.Empty, typeof(IPurchaseOrder))
            };

            return purchaseOrder;
        }
    }
}