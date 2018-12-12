using System;

namespace EPiServer.Commerce.OrderStore.Model
{
    [Flags]
    public enum ReturnFormStatusModel
    {
        Complete = 1,
        Canceled = 2,
        AwaitingStockReturn = 4,
        AwaitingCompletion = 8
    }
}
