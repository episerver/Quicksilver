using Mediachase.Commerce.Engine;
using Mediachase.Commerce.Inventory;
using System.Collections.Generic;

namespace Mediachase.Commerce.Workflow.PurchaseOrderActivities
{
    public abstract class HandoffActivityBase : OrderGroupActivityBase
    {
        [ActivityFlowContextProperty]
        public Dictionary<int, IWarehouse> PickupWarehouseInShipment { get; set; }
    }
}