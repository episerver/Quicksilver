using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace EPiServer.Commerce.Orders.Cloud.Client.OrderModel
{
    internal static class NotifyCollectionChangedEventArgsExtensions
    {
        internal static IEnumerable<T> GetNewItems<T>(this NotifyCollectionChangedEventArgs args)
        {
            if (args == null ||
                (args.Action != NotifyCollectionChangedAction.Add &&
                 args.Action != NotifyCollectionChangedAction.Replace))
            {
                return Enumerable.Empty<T>();
            }

            return args.NewItems.OfType<T>();
        }
    }
}
