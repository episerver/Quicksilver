using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;

namespace EPiServer.Commerce.Orders.Cloud.Client
{
    class DefaultOrderPostProcessor : IOrderPostProcessor
    {
        readonly IPurchaseOrderProcessor _purchaseOrderProcessor;
        readonly IOrderRepository _orderRepository;

        public DefaultOrderPostProcessor(IPurchaseOrderProcessor purchaseOrderProcessor,
            IOrderRepository orderRepository)
        {
            _purchaseOrderProcessor = purchaseOrderProcessor;
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<string>> ProcessOrderAsync(IPurchaseOrder order)
        {
            var result = _purchaseOrderProcessor.ProcessOrder(order);
            return await HandleIssues(order, result);
        }

        async Task<IEnumerable<string>> HandleIssues(IPurchaseOrder order, OrderProcessingResult result)
        {
            var messages = new List<string>();
            messages.Add($"Processing Order {order.OrderNumber}");

            if (!result.Result.Any())
            {
                _orderRepository.Save(order);
                messages.Add($"Order saved, no issues found.");
                return messages;
            }

            var isValidOrder = true;

            foreach (var validationIssue in result.Result)
            {
                foreach (var issue in validationIssue.Value)
                {
                    switch (issue)
                    {
                        case ValidationIssue.None:
                        case ValidationIssue.RemovedGiftDueToInsufficientQuantityInInventory:
                            break;
                        case ValidationIssue.CannotProcessDueToMissingOrderStatus:
                            messages.Add($"Cannot process product {validationIssue.Key.Code} because of missing order status.'");
                            isValidOrder = false;
                            break;
                        case ValidationIssue.RemovedDueToCodeMissing:
                        case ValidationIssue.RemovedDueToNotAvailableInMarket:
                        case ValidationIssue.RemovedDueToInactiveWarehouse:
                        case ValidationIssue.RemovedDueToMissingInventoryInformation:
                        case ValidationIssue.RemovedDueToUnavailableCatalog:
                        case ValidationIssue.RemovedDueToUnavailableItem:
                            messages.Add($"The product {validationIssue.Key.Code} is not available in store and was removed from your order.");
                            break;
                        case ValidationIssue.RemovedDueToInsufficientQuantityInInventory:
                            messages.Add($"The product {validationIssue.Key.Code} is sold out and was removed from your order.");
                            break;
                        case ValidationIssue.RemovedDueToInvalidPrice:
                            messages.Add($"The product {validationIssue.Key.Code} does not have a valid price and was removed from your order.");
                            break;
                        case ValidationIssue.AdjustedQuantityByMinQuantity:
                        case ValidationIssue.AdjustedQuantityByMaxQuantity:
                        case ValidationIssue.AdjustedQuantityByBackorderQuantity:
                        case ValidationIssue.AdjustedQuantityByPreorderQuantity:
                        case ValidationIssue.AdjustedQuantityByAvailableQuantity:
                            messages.Add($"The quantity of product {validationIssue.Key.Code} has changed.");
                            break;
                        case ValidationIssue.PlacedPricedChanged:
                            messages.Add($"The price for product {validationIssue.Key.Code} has changed since it was added to your order.");
                            break;
                        default:
                            messages.Add("Failed to process your order.");
                            isValidOrder = false;
                            break;
                    }
                }
            }

            if (isValidOrder)
            {
                _orderRepository.Save(order);
                messages.Add($"Order saved.");
            }
            else
            {
                messages.Add($"Order not saved due to issues.");
            }

            return messages;
        }
    }
}
