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
    public class CloudPaymentPlanProvider : IPaymentPlanProvider
    {
        readonly PaymentPlanClient<CloudyPaymentPlan> _paymentPlanClient;
        readonly ICurrentMarket _currentMarket;
        readonly ServiceAccessor<SiteContext> _siteContextAccessor;
        readonly IOrderGroupFactory _orderGroupFactory;
        readonly IPurchaseOrderProvider _purchaseOrderProvider;
        readonly IOrderNumberGenerator _orderNumberGenerator;
        readonly ICouponUsage _couponUsage;

        public CloudPaymentPlanProvider(
            PaymentPlanClient<CloudyPaymentPlan> paymentPlanClient, 
            ICurrentMarket currentMarket, 
            ServiceAccessor<SiteContext> siteContextAccessor, 
            IOrderGroupFactory orderGroupFactory, 
            IPurchaseOrderProvider purchaseOrderProvider, 
            IOrderNumberGenerator orderNumberGenerator, 
            ICouponUsage couponUsage)
        {
            _paymentPlanClient = paymentPlanClient;
            _currentMarket = currentMarket;
            _siteContextAccessor = siteContextAccessor;
            _orderGroupFactory = orderGroupFactory;
            _purchaseOrderProvider = purchaseOrderProvider;
            _orderNumberGenerator = orderNumberGenerator;
            _couponUsage = couponUsage;
        }

        public IPaymentPlan Create(Guid customerId, string name)
        {
            return CreatePaymentPlan(customerId, name);
        }

        public IPaymentPlan Load(int orderGroupId)
        {
            return  _paymentPlanClient.Load(orderGroupId);
        }

        public IEnumerable<IPaymentPlan> Load(Guid customerId, string name)
        {
            return  _paymentPlanClient.Load(customerId, name);
        }

        public OrderReference Save(IPaymentPlan paymentPlan)
        {
            paymentPlan.GetOrderGroupTotals();
            var savedPaymentPlan = _paymentPlanClient.Save(paymentPlan).OrderLink;
            ((CloudyPaymentPlan)paymentPlan).OrderGroupId = savedPaymentPlan.OrderGroupId;
            return savedPaymentPlan;
        }

        public void Delete(OrderReference orderLink)
        {
             _paymentPlanClient.Delete(orderLink.OrderGroupId);
        }

        public IPurchaseOrder SaveAsPurchaseOrder(IPaymentPlan paymentPlan)
        {
            var purchaseOrder = _purchaseOrderProvider.Create(paymentPlan.CustomerId, paymentPlan.Name);
            purchaseOrder.CopyFrom(paymentPlan, _orderGroupFactory);

            if (purchaseOrder.Properties.ContainsKey("OrderNumber"))
            {
                purchaseOrder.OrderNumber = (string)purchaseOrder.Properties["OrderNumber"];
            }
            else
            {
                purchaseOrder.OrderNumber = _orderNumberGenerator.GenerateOrderNumber(paymentPlan);
            }

            foreach (var form in purchaseOrder.Forms)
            {
                _couponUsage.Report(form.Promotions);
            }

            var orderReference = _purchaseOrderProvider.Save(purchaseOrder);

            return _purchaseOrderProvider.Load(orderReference.OrderGroupId);
        }

        CloudyPaymentPlan CreatePaymentPlan(Guid customerId, string name)
        {
            var market = _currentMarket.GetCurrentMarket();
            var paymentPlan = new CloudyPaymentPlan
            {
                CustomerId = customerId,
                Name = name,
                MarketId = market.MarketId,
                MarketName = market.MarketName,
                PricesIncludeTax = market.PricesIncludeTax,
                Currency = _siteContextAccessor().Currency,
            };
            
            var orderForm = paymentPlan.CreateOrderForm(_orderGroupFactory);
            orderForm.Name = paymentPlan.Name;

            var shipment = paymentPlan.CreateShipment(_orderGroupFactory);
            shipment.ShippingMethodId = Guid.Empty;

            orderForm.Shipments.Add(shipment);
            paymentPlan.Forms.Add(orderForm);

            return paymentPlan;
        }
    }
}
