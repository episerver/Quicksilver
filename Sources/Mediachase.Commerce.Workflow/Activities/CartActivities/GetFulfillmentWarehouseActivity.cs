using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Workflow.Activities.PurchaseOrderActivities;
using Mediachase.Commerce.WorkflowCompatibility;
using System.Collections.Generic;
using System.Linq;

namespace Mediachase.Commerce.Workflow.Customization.Cart
{
    public class GetFulfillmentWarehouseActivity : HandoffActivityBase
    {
        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            // Validate the properties at runtime
            ValidateRuntime();
            GetFulfillmentWarehouse();
            return ActivityExecutionStatus.Closed;
        }

        private void GetFulfillmentWarehouse()
        {
            foreach (OrderForm form in OrderGroup.OrderForms)
            {
                var deletedLineItems = new List<LineItem>();
                foreach (var shipment in form.Shipments.OfType<Shipment>())
                {
                    var warehouse = FulfillmentWarehouseProcessor.GetFulfillmentWarehouse(shipment);
                    foreach (var lineItem in Shipment.GetShipmentLineItems(shipment))
                    {
                        if (warehouse != null)
                        {
                            lineItem.InventoryStatus = (int)GetInventoryStatus(lineItem.Code, warehouse.Code);
                        }
                        else if (CatalogContext.Current.GetCatalogEntryDto(lineItem.Code, new CatalogEntryResponseGroup(CatalogEntryResponseGroup.ResponseGroup.Variations)) != null)
                        {
                            deletedLineItems.Add(lineItem);
                        }
                    }

                    if (warehouse != null)
                    {
                        shipment.WarehouseCode = warehouse.Code;
                    }
                    
                }
                foreach (var lineItem in deletedLineItems)
                {
                    form.RemoveLineItemFromShipments(lineItem);
                    lineItem.Delete();
                }
            }
        }
    }
}