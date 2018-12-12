using System.Collections.Generic;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;

namespace EPiServer.Commerce.Orders.Cloud.Client
{
    public interface IOrderPostProcessor
    {
        Task<IEnumerable<string>> ProcessOrderAsync(IPurchaseOrder order); 
    }
}
