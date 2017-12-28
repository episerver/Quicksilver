using System;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using EPiServer.ServiceLocation;

namespace EPiServer.Reference.Commerce.Site.Infrastructure
{
    [ServiceConfiguration(typeof(IOrderRepositoryCallback), Lifecycle = ServiceInstanceScope.Singleton)]
    public class SiteOrderRepositoryCallback : IOrderRepositoryCallback
    {
        private readonly ILogger _logger = LogManager.GetLogger();

        public void OnCreating(Guid customerId, string name)
        {
            _logger.Information($"Creating order: customer [{customerId}], name[{name}].");
        }

        public void OnCreated(OrderReference orderReference)
        {
            _logger.Information($"Created order {orderReference.OrderType}: orderid [{orderReference.OrderGroupId}], customer [{orderReference.CustomerId}], name[{orderReference.Name}].");
        }
        
        public void OnUpdating(OrderReference orderReference)
        {
            _logger.Information($"Updating order {orderReference.OrderType}: orderid [{orderReference.OrderGroupId}], customer [{orderReference.CustomerId}], name[{orderReference.Name}].");
        }

        public void OnUpdated(OrderReference orderReference)
        {
            _logger.Information($"Updated order {orderReference.OrderType}: orderid [{orderReference.OrderGroupId}], customer [{orderReference.CustomerId}], name[{orderReference.Name}].");
        }

        public void OnDeleting(OrderReference orderReference)
        {
            _logger.Information($"Deleting order {orderReference.OrderType}: orderid [{orderReference.OrderGroupId}], customer [{orderReference.CustomerId}], name[{orderReference.Name}].");
        }

        public void OnDeleted(OrderReference orderReference)
        {
            _logger.Information($"Deleted order {orderReference.OrderType}: orderid [{orderReference.OrderGroupId}], customer [{orderReference.CustomerId}], name[{orderReference.Name}].");
        }
    }
}