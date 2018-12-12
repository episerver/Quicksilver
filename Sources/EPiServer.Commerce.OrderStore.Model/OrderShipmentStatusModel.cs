using System;

namespace EPiServer.Commerce.OrderStore.Model
{
    [Flags]
    public enum OrderShipmentStatusModel
    {
        AwaitingInventory = 1,
        Cancelled = 2,
        InventoryAssigned = 4,
        OnHold = 8,
        Packing = 16,
        Released = 32,
        Shipped = 64
    }
}