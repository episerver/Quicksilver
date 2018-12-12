using EPiServer.Commerce.Order;
using System;
using System.Linq;

namespace EPiServer.Commerce.Orders.Cloud.Client.OrderModel
{
    [Serializable]
    public class CloudyOrderNote : IOrderNote
    {
        IOrderGroup _parent;

        public int? OrderNoteId { get; set; }

        public string Title { get; set; }

        public string Detail { get; set; }

        public string Type { get; set; }

        public int? LineItemId { get; set; }

        public Guid CustomerId { get; set; }

        public DateTime Created { get; set; }

        internal void SetParentOrder(CloudyOrderGroupBase newParent, bool keepIds)
        {
            if (!keepIds && newParent != _parent)
            {
                var oderNotesId = newParent.Notes.Select(x => x.OrderNoteId);
                OrderNoteId = (oderNotesId.Max() == null ? 0 : oderNotesId.Max())+1;
            }
            _parent = newParent;
        }
    }
}