using System;

namespace EPiServer.Commerce.OrderStore.Model
{
    public class OrderNoteModel
    {
        public string Title { get; set; }
        public string Detail { get; set; }
        public string Type { get; set; }
        public int? LineItemId { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime Created { get; set; }       
        public int? OrderNoteId { get; set; }
    }
}