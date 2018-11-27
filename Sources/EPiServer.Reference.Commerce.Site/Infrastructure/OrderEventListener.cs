using EPiServer.Commerce.Order;
using EPiServer.Logging;

namespace EPiServer.Reference.Commerce.Site.Infrastructure
{
    public class OrderEventListener
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderEvents _orderEvents;
        private readonly ILogger _logger = LogManager.GetLogger(typeof(OrderEventListener));

        public OrderEventListener(IOrderRepository orderRepository, IOrderEvents orderEvents)
        {
            _orderRepository = orderRepository;
            _orderEvents = orderEvents;
        }

        public void AddEvents()
        {
            _orderEvents.SavedOrder += OrderEventsOnSavedOrder;
            _orderEvents.DeletingOrder += OrderEventsOnDeletingOrder;
        }

        private void OrderEventsOnSavedOrder(object sender, OrderEventArgs orderEventArgs)
        {
            var po = orderEventArgs.OrderGroup as IPurchaseOrder;
            if (po != null)
            {
                _logger.Information($"Order {po.OrderNumber} was saved");
            }
        }

        private void OrderEventsOnDeletingOrder(object sender, OrderEventArgs orderEventArgs)
        {
            var cart = _orderRepository.Load<ICart>(orderEventArgs.OrderLink.OrderGroupId);
            if (cart != null)
            {
                _logger.Information($"Cart '{cart.Name}' for user '{cart.CustomerId}' is being deleted.");
            }
        }

        public void RemoveEvents()
        {
            _orderEvents.DeletingOrder -= OrderEventsOnDeletingOrder;
            _orderEvents.SavedOrder -= OrderEventsOnSavedOrder;
        }
    }
}