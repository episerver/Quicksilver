using EPiServer.Commerce.Order;
using EPiServer.Commerce.Orders.Cloud.Client.OrderModel;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Core;
using System;
using System.Collections.Generic;

namespace EPiServer.Commerce.Orders.Cloud.Client.Providers
{
    public class CloudPurchaseOrderProvider : IPurchaseOrderProvider, IPurchaseOrderRepository
    {
        readonly OrderClient<CloudyOrder> _orderClient;
        readonly ICurrentMarket _currentMarket;
        readonly ServiceAccessor<SiteContext> _siteContextAccessor;
        readonly IOrderGroupFactory _orderGroupFactory;

        public CloudPurchaseOrderProvider(
            OrderClient<CloudyOrder> orderClient, 
            ICurrentMarket currentMarket, 
            ServiceAccessor<SiteContext> siteContextAccessor, 
            IOrderGroupFactory orderGroupFactory)
        {
            _orderClient = orderClient;
            _currentMarket = currentMarket;
            _siteContextAccessor = siteContextAccessor;
            _orderGroupFactory = orderGroupFactory;
        }

        public IPurchaseOrder Create(Guid customerId, string name)
        {
            var market = _currentMarket.GetCurrentMarket();
            var order = new CloudyOrder
            {
                CustomerId = customerId,
                Name = name,
                MarketId = market.MarketId,
                MarketName = market.MarketName,
                PricesIncludeTax = market.PricesIncludeTax,
                Currency = _siteContextAccessor().Currency
            };
            
            var orderForm = order.CreateOrderForm(_orderGroupFactory);
            orderForm.Name = order.Name;

            var shipment = order.CreateShipment(_orderGroupFactory);
            shipment.ShippingMethodId = Guid.Empty;

            orderForm.Shipments.Add(shipment);
            order.Forms.Add(orderForm);

            return order;
        }

        public void Delete(OrderReference orderLink)
        {
            _orderClient.Delete(orderLink.OrderGroupId);
        }

        public IPurchaseOrder Load(int orderGroupId)
        {
            return _orderClient.Load( orderGroupId);
        }

        public IEnumerable<IPurchaseOrder> Load(Guid customerId, string name)
        {
            return _orderClient.Load(customerId,name);
        }

        public OrderReference Save(IPurchaseOrder purchaseOrder)
        {
            purchaseOrder.GetOrderGroupTotals();

            var savedPurchaseOrder =  _orderClient.Save(purchaseOrder).OrderLink;
            ((CloudyOrder)purchaseOrder).OrderGroupId = savedPurchaseOrder.OrderGroupId;

            return savedPurchaseOrder;
        }

        public IPurchaseOrder Load(string trackingNumber)
        {
            return _orderClient.Load(trackingNumber);
        }
    }
}
