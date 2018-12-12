using System;

namespace EPiServer.Commerce.OrderStore.Model
{
    [Flags]
    public enum OrderStatusModel
    {
        OnHold = 1,
        PartiallyShipped = 2,
        InProgress = 4,
        Completed = 8,
        Cancelled = 16,
        AwaitingExchange = 32
    }
}