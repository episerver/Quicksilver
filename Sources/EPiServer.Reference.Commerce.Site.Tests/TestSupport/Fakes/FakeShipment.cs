using EPiServer.Commerce.Order;
using Mediachase.Commerce.Orders;
using System;
using System.Collections;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Tests.TestSupport.Fakes
{
    public class FakeShipment : IShipment, IShipmentDiscountAmount
    {
        private static int _counter;

        public FakeShipment()
        {
            LineItems = new List<ILineItem>();
            ShipmentId = ++_counter;
            Properties = new Hashtable();
        }

        public int ShipmentId { get; set; }

        public Guid ShippingMethodId { get; set; }

        public string ShippingMethodName { get; set; }

        public IOrderAddress ShippingAddress { get; set; }

        public string ShipmentTrackingNumber { get; set; }

        public OrderShipmentStatus OrderShipmentStatus { get; set; }

        public string Status { get; set; }

        public int? PickListId { get; set; }

        public string WarehouseCode { get; set; }

        public ICollection<ILineItem> LineItems { get; set; }

        public decimal ShipmentDiscount { get; set; }

        public Hashtable Properties { get; private set; }

        /// <inheritdoc />
        public IOrderGroup ParentOrderGroup { get; set; }

        public static FakeShipment CreatShipment(int id, IOrderAddress orderAddress, decimal discount, IList<ILineItem> items, string shippingMethodIdString = null, Hashtable properties = null)
        {
            return new FakeShipment
            {
                ShipmentId = id,
                ShippingAddress = orderAddress,
                ShippingMethodId = new Guid(shippingMethodIdString ?? "7eedee57-c8f4-4d19-a58c-284e72094527"),
                ShipmentDiscount = discount,
                WarehouseCode = "default",
                LineItems = items,
                Properties = properties ?? new Hashtable()

            };
        }

        public static FakeShipment CreatShipment(int id, Guid shippingMethodId, IOrderAddress shippingAddress, IList<ILineItem> items = null, Hashtable properties = null)
        {
            return new FakeShipment
            {
                ShipmentId = id,
                ShippingAddress = shippingAddress,
                ShippingMethodId = shippingMethodId,
                ShipmentDiscount = 0,
                WarehouseCode = "default",
                LineItems = items ?? new List<ILineItem>(),
                Properties = properties ?? new Hashtable()
            };
        }

        public static FakeShipment CreatShipment()
        {
            return new FakeShipment
            {
                ShipmentId = 1,
                ShipmentDiscount = 0,
                WarehouseCode = "default"
            };
        }

    }
}
