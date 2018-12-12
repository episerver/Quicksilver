using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Orders.Cloud.Client.OrderModel;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Core;
using System;
using System.Collections.Generic;

namespace EPiServer.Commerce.Orders.Cloud.Client.Providers
{
    [ServiceConfiguration(IncludeServiceAccessor = false)]
    public class CloudCartProvider : ICartProvider
    {
        readonly ICurrentMarket _currentMarket;
        readonly ServiceAccessor<SiteContext> _siteContextAccessor;
        readonly IOrderGroupFactory _orderGroupFactory;
        readonly CartClient<CloudyCart> _cartClient;
        readonly IPurchaseOrderProvider _purchaseOrderProvider;
        readonly IPaymentPlanProvider _paymentPlanProvider;
        readonly IOrderNumberGenerator _orderNumberGenerator;
        readonly ICouponUsage _couponUsage;

        public CloudCartProvider(
            ICurrentMarket currentMarket,
            ServiceAccessor<SiteContext> siteContextAccessor,
            IOrderGroupFactory orderGroupFactory,
            CartClient<CloudyCart> cartClient,
            IPurchaseOrderProvider purchaseOrderProvider, 
            IOrderNumberGenerator orderNumberGenerator, 
            IPaymentPlanProvider paymentPlanProvider, 
            ICouponUsage couponUsage)
        {
            _currentMarket = currentMarket;
            _siteContextAccessor = siteContextAccessor;
            _orderGroupFactory = orderGroupFactory;
            _cartClient = cartClient;
            _purchaseOrderProvider = purchaseOrderProvider;
            _orderNumberGenerator = orderNumberGenerator;
            _paymentPlanProvider = paymentPlanProvider;
            _couponUsage = couponUsage;
        }

        public ICart Create(Guid customerId, string name)
        {
            return CreateCart(customerId, name);
        }

        public void Delete(OrderReference orderLink)
        {
             _cartClient.Delete(orderLink.OrderGroupId);
        }

        public ICart Load(int orderGroupId)
        {
            return _cartClient.Load(orderGroupId);
        }

        public IEnumerable<ICart> Load(Guid customerId, string name)
        {
            return _cartClient.Load(customerId, name);
        }

        public OrderReference Save(ICart cart)
        {
            cart.GetOrderGroupTotals();
            var savedCart =  _cartClient.Save(cart).OrderLink;
            ((CloudyCart) cart).OrderGroupId = savedCart.OrderGroupId;
            return savedCart;
        }

        public IPaymentPlan SaveAsPaymentPlan(ICart cart)
        {
            var paymentPlan = _paymentPlanProvider.Create(cart.CustomerId, cart.Name);
            paymentPlan.CopyFrom(cart, _orderGroupFactory);

            var orderReference = _paymentPlanProvider.Save(paymentPlan);
            return _paymentPlanProvider.Load(orderReference.OrderGroupId);
        }

        public IPurchaseOrder SaveAsPurchaseOrder(ICart cart)
        {
            var purchaseOrder = _purchaseOrderProvider.Create(cart.CustomerId, cart.Name);
            purchaseOrder.CopyFrom(cart, _orderGroupFactory);

            if (purchaseOrder.Properties.ContainsKey("OrderNumber"))
            {
                purchaseOrder.OrderNumber = (string)purchaseOrder.Properties["OrderNumber"];
            }
            else
            {
                purchaseOrder.OrderNumber = _orderNumberGenerator.GenerateOrderNumber(cart);
            }

            foreach (var form in purchaseOrder.Forms)
            {
                _couponUsage.Report(form.Promotions);
            }

            var orderReference = _purchaseOrderProvider.Save(purchaseOrder);
            return _purchaseOrderProvider.Load(orderReference.OrderGroupId);
        }

        CloudyCart CreateCart(Guid customerId, string name)
        {
            var market = _currentMarket.GetCurrentMarket();
            var cart = new CloudyCart
            {
                CustomerId = customerId,
                Name = name,
                MarketId = market.MarketId,
                MarketName = market.MarketName,
                PricesIncludeTax = market.PricesIncludeTax,
                Currency = _siteContextAccessor().Currency
            };
            
            var orderForm = cart.CreateOrderForm(_orderGroupFactory);
            orderForm.Name = cart.Name;

            var shipment = cart.CreateShipment(_orderGroupFactory);
            shipment.ShippingMethodId = Guid.Empty;

            orderForm.Shipments.Add(shipment);
            cart.Forms.Add(orderForm);

            return cart;
        }
    }
}
