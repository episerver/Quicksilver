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
            _logger.Information(string.Format("Creating order: customer [{0}], name[{1}].", customerId, name));
        }

        public void OnCreated(OrderReference orderReference)
        {
            _logger.Information(string.Format("Created order {0}: orderid [{1}], customer [{2}], name[{3}].", orderReference.OrderType, orderReference.OrderGroupId, orderReference.CustomerId, orderReference.Name));
        }
        
        public void OnUpdating(OrderReference orderReference)
        {
            _logger.Information(string.Format("Updating order {0}: orderid [{1}], customer [{2}], name[{3}].", orderReference.OrderType, orderReference.OrderGroupId, orderReference.CustomerId, orderReference.Name));
        }

        public void OnUpdated(OrderReference orderReference)
        {
            _logger.Information(string.Format("Updated order {0}: orderid [{1}], customer [{2}], name[{3}].", orderReference.OrderType, orderReference.OrderGroupId, orderReference.CustomerId, orderReference.Name));
        }

        public void OnDeleting(OrderReference orderReference)
        {
            _logger.Information(string.Format("Deleting order {0}: orderid [{1}], customer [{2}], name[{3}].", orderReference.OrderType, orderReference.OrderGroupId, orderReference.CustomerId, orderReference.Name));
        }

        public void OnDeleted(OrderReference orderReference)
        {
            _logger.Information(string.Format("Deleted order {0}: orderid [{1}], customer [{2}], name[{3}].", orderReference.OrderType, orderReference.OrderGroupId, orderReference.CustomerId, orderReference.Name));
        }
    }
}